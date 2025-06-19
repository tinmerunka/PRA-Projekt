/**
 * Simplified Quiz Player Module
 */
onDocumentReady(() => {
    if (!requireAuth()) return;

    // Get the quiz ID from URL params or localStorage
    const urlParams = new URLSearchParams(window.location.search);
    const quizId = urlParams.get('quiz') || localStorage.getItem('quizId');
    const username = localStorage.getItem('username') || 'Player';

    // Add this at the top of your onDocumentReady function
    const token = localStorage.getItem('token');
    if (!token) {
        console.error("No authentication token found");
        window.location.href = '/pages/auth/login.html?redirect=' + encodeURIComponent(window.location.href);
        return;
    }

    // Log the authentication state
    console.log("Authentication state:", {
        token: !!token,
        username: username
    });

    // State variables
    let currentQuestion = null;
    let currentQuestionIndex = 0;
    let totalQuestions = 0;
    let timeLeft = 0;
    let timerInterval = null;
    let playerId = null;
    let sessionId = null;
    let score = 0;

    // Page elements
    const quizInfoContainer = document.getElementById('quizInfoContainer');
    const gameContainer = document.getElementById('gameContainer');
    const resultContainer = document.getElementById('resultContainer');

    // Initialize
    initQuizPage();

    function initQuizPage() {
        // Validate quiz ID
        if (!quizId) {
            alert('No quiz selected. Redirecting to quiz list.');
            window.location.href = '/pages/quiz/list.html';
            return;
        }

        // Load quiz info
        fetchQuizInfo();
    }

    function fetchQuizInfo() {
        // Show loading state
        quizInfoContainer.innerHTML = '<div class="loading">Loading quiz information...</div>';

        // Fetch quiz details
        fetch(`/api/quiz/${quizId}/info`)
            .then(response => {
                if (!response.ok) throw new Error('Quiz not found');
                return response.json();
            })
            .then(quizInfo => {
                totalQuestions = quizInfo.totalQuestions;

                // Display quiz info with start button
                quizInfoContainer.innerHTML = `
                    <div class="quiz-info-card">
                        <h2>${quizInfo.title}</h2>
                        <p>${quizInfo.description}</p>
                        <div class="quiz-meta">
                            <span>${totalQuestions} questions</span>
                        </div>
                        <button id="btnStartQuiz" class="btn btn-primary">Start Quiz</button>
                    </div>
                `;

                // Add click handler for start button
                document.getElementById('btnStartQuiz').addEventListener('click', startQuiz);
            })
            .catch(error => {
                console.error('Error loading quiz:', error);
                quizInfoContainer.innerHTML = `
                    <div class="error-message">
                        <h2>Error Loading Quiz</h2>
                        <p>${error.message}</p>
                        <button onclick="window.location.href='/pages/quiz/list.html'" class="btn btn-secondary">
                            Return to Quiz List
                        </button>
                    </div>
                `;
            });
    }

    function startQuiz() {
        // Add loading state to button
        const startButton = document.getElementById('btnStartQuiz');
        const originalText = startButton.textContent;
        startButton.disabled = true;
        startButton.textContent = 'Starting...';

        // Get the current user's info from the token
        const tokenData = decodeToken(token);
        console.log("Token data:", tokenData);

        // Try multiple sources for the username
        let playerName;

        // First try localStorage username
        if (username && username !== 'undefined' && username !== 'null') {
            playerName = username;
        }
        // Then try token data
        else if (tokenData) {
            playerName = tokenData.unique_name || tokenData.sub || tokenData.Username || tokenData.username;
        }

        // If all else fails, generate a random name
        if (!playerName || playerName === 'undefined' || playerName === 'null') {
            playerName = 'Player_' + Math.floor(Math.random() * 10000);
        }

        console.log("Using player name:", playerName);

        // Register player and start quiz
        const requestBody = {
            playerName: playerName,
            quizId: parseInt(quizId)
        };

        console.log("Starting quiz with data:", JSON.stringify(requestBody));

        fetch('/api/quiz/start', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(requestBody)
        })
            .then(response => {
                if (!response.ok) {
                    console.error("Server responded with status:", response.status);
                    throw new Error('Server error: ' + response.statusText);
                }
                return response.json();
            })
            .then(data => {
                console.log("Quiz start response:", data);

                // Store player ID and session
                playerId = data.playerId;
                sessionId = data.sessionId;

                console.log(`Quiz started with playerId: ${playerId}, sessionId: ${sessionId}`);

                if (playerId === -1) {
                    // This is a common error from DbServices.AddNewPlayer
                    throw new Error('Database could not create player record. Please try again later.');
                } else if (!playerId || playerId <= 0) {
                    throw new Error('Invalid player ID received from server');
                }

                // Hide quiz info, show game
                quizInfoContainer.style.display = 'none';
                gameContainer.style.display = 'flex';

                // Load first question
                loadQuestion(1);
            })
            .catch(error => {
                console.error('Error starting quiz:', error);

                // Show helpful error message with retry option
                quizInfoContainer.innerHTML = `
                <div class="error-message">
                    <h2>Error Starting Quiz</h2>
                    <p>${error.message}</p>
                    <div class="button-group">
                        <button onclick="window.location.reload()" class="btn btn-primary">
                            Try Again
                        </button>
                        <a href="/pages/quiz/list.html" class="btn btn-secondary">
                            Return to Quiz List
                        </a>
                    </div>
                </div>
            `;

                // Fix the variable reference
                startButton.disabled = false;
                startButton.textContent = originalText;
            });
    }

    function loadQuestion(position) {
        currentQuestionIndex = position;

        fetch(`/api/quiz/${quizId}/questions/${position}`)
            .then(response => {
                if (!response.ok) throw new Error('Question not found');
                return response.json();
            })
            .then(question => {
                showQuestion(question);
            })
            .catch(error => {
                console.error('Error loading question:', error);
                alert('Failed to load question. Moving to next one.');
                nextQuestion();
            });
    }

    function showQuestion(question) {
        currentQuestion = question;
        timeLeft = question.questionTime || 30;

        // Update question text and position
        document.getElementById('questionText').textContent = question.questionText;
        document.getElementById('currentQuestionNum').textContent = question.questionPosition;
        document.getElementById('totalQuestions').textContent = totalQuestions;

        // Update answer options
        const answerButtons = document.querySelectorAll('.answer-card');
        const answers = question.answers || [];

        answerButtons.forEach((button, index) => {
            if (index < answers.length) {
                const answerText = button.querySelector('.answer-text');
                if (answerText) {
                    answerText.textContent = answers[index].answerText;
                }

                // Reset button state
                button.classList.remove('correct', 'incorrect', 'selected');
                button.disabled = false;
                button.style.display = 'flex';

                // Add click handler
                button.onclick = () => selectAnswer(index);
            } else {
                // Hide extra buttons if fewer than 4 answers
                button.style.display = 'none';
            }
        });

        // Start the timer
        startTimer();
    }

    function startTimer() {
        const timerText = document.getElementById('timerText');
        const timerProgress = document.querySelector('.timer-progress');

        // Clear any existing interval
        if (timerInterval) {
            clearInterval(timerInterval);
        }

        // Update timer initially
        updateTimerDisplay();

        // Set up the interval
        timerInterval = setInterval(() => {
            timeLeft--;
            updateTimerDisplay();

            if (timeLeft <= 0) {
                clearInterval(timerInterval);
                disableAnswerButtons();
                setTimeout(() => {
                    showCorrectAnswer();
                    setTimeout(nextQuestion, 2000);
                }, 1000);
            }
        }, 1000);
    }

    function updateTimerDisplay() {
        const timerText = document.getElementById('timerText');
        const timerProgress = document.querySelector('.timer-progress');

        if (timerText) {
            timerText.textContent = timeLeft;
        }

        if (timerProgress) {
            const maxTime = currentQuestion.questionTime || 30;
            const progressPercentage = (timeLeft / maxTime) * 100;
            timerProgress.style.width = `${progressPercentage}%`;
        }
    }

    function selectAnswer(index) {
        // Verify we have a valid player ID
        if (!playerId || playerId <= 0) {
            console.error("Invalid player ID:", playerId);
            alert("There was an error with your player session. Please restart the quiz.");
            return;
        }

        // Disable all buttons
        disableAnswerButtons();

        // Mark the selected button
        const answerButtons = document.querySelectorAll('.answer-card');
        if (answerButtons[index]) {
            answerButtons[index].classList.add('selected');
        }

        // Stop the timer
        clearInterval(timerInterval);

        // Create the request data
        const answerData = {
            playerId: playerId,
            questionId: currentQuestionIndex, // Use the current position (1-based)
            answerIndex: index,
            timeRemaining: timeLeft
        };

        console.log("Submitting answer:", answerData);

        // Send the answer to the server
        fetch(`/api/quiz/${quizId}/submit-answer`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(answerData)
        })
            .then(response => {
                console.log("Submit answer response status:", response.status);

                if (!response.ok) {
                    throw new Error(`HTTP error! Status: ${response.status}`);
                }

                // Try to parse as JSON
                return response.json();
            })
            .then(result => {
                console.log("Answer submission result:", result);

                // Update score
                score += result.score || 0;

                // Show correct answer
                showCorrectAnswer(result.correctAnswerIndex);

                // Move to next question after delay
                setTimeout(nextQuestion, 2000);
            })
            .catch(error => {
                console.error('Error submitting answer:', error);

                // Try to continue the quiz anyway by finding correct answer locally
                let correctAnswerIndex = -1;
                if (currentQuestion && currentQuestion.answers) {
                    correctAnswerIndex = currentQuestion.answers.findIndex(a => a.correct);
                }

                showCorrectAnswer(correctAnswerIndex);
                setTimeout(nextQuestion, 2000);
            });
    }

    function showCorrectAnswer(correctIndex) {
        const answerButtons = document.querySelectorAll('.answer-card');

        // Find correct answer index if not provided
        if (correctIndex === undefined && currentQuestion && currentQuestion.answers) {
            correctIndex = currentQuestion.answers.findIndex(a => a.correct);
        }

        // Highlight correct/incorrect answers
        if (correctIndex !== undefined && correctIndex >= 0) {
            answerButtons.forEach((button, index) => {
                if (index === correctIndex) {
                    button.classList.add('correct');
                } else if (button.classList.contains('selected')) {
                    button.classList.add('incorrect');
                }
            });
        }
    }

    function disableAnswerButtons() {
        document.querySelectorAll('.answer-card').forEach(button => {
            button.disabled = true;
        });
    }

    function nextQuestion() {
        if (currentQuestionIndex < totalQuestions) {
            loadQuestion(currentQuestionIndex + 1);
        } else {
            endQuiz();
        }
    }

    function endQuiz() {
        // Hide game container
        gameContainer.style.display = 'none';

        // Show results
        resultContainer.style.display = 'flex';
        resultContainer.innerHTML = '<div class="loading">Loading results...</div>';

        // Fetch final results
        fetch(`/api/quiz/player/${playerId}/results/${quizId}`)
            .then(response => {
                if (!response.ok) throw new Error('Failed to get results');
                return response.json();
            })
            .then(results => {
                console.log("Final results:", results); // Debug log
                resultContainer.innerHTML = `
                    <div class="results-card">
                        <h2>Quiz Complete!</h2>
                        <div class="final-score">
                            <h3>Your Score</h3>
                            <div class="score-display">${results.score !== undefined ? results.score : score} points</div>
                        </div>
                        <div class="result-summary">
                            <p>You completed ${totalQuestions} questions from "${results.quizTitle || 'Quiz'}"</p>
                        </div>
                        <div class="result-actions">
                            <button onclick="window.location.href='/pages/quiz/list.html'" class="btn btn-primary">
                                Play Another Quiz
                            </button>
                            <button onclick="window.location.reload()" class="btn btn-secondary">
                                Try Again
                            </button>
                        </div>
                    </div>
                `;
            })
            .catch(error => {
                console.error('Error loading results:', error);
                // Show basic results without server data
                resultContainer.innerHTML = `
                    <div class="results-card">
                        <h2>Quiz Complete!</h2>
                        <div class="final-score">
                            <h3>Your Score</h3>
                            <div class="score-display">${score} points</div>
                        </div>
                        <div class="result-summary">
                            <p>You completed ${totalQuestions} questions</p>
                        </div>
                        <div class="result-actions">
                            <button onclick="window.location.href='/pages/quiz/list.html'" class="btn btn-primary">
                                Play Another Quiz
                            </button>
                            <button onclick="window.location.reload()" class="btn btn-secondary">
                                Try Again
                            </button>
                        </div>
                    </div>
                `;
            });
    }
});
