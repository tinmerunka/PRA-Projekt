/**
 * Quiz Creator Module
 */
onDocumentReady(() => {
    if (!requireAuth()) return;

    // Elements
    const quizName = document.getElementById('quizTitle');
    const quizDescription = document.getElementById('quizDescription');
    const quizPanel = document.querySelector('.quiz-panel');
    const questionPanel = document.querySelector('.question-panel');
    const btnAddAnotherQuestion = document.getElementById('btnAddAnotherQuestion');
    const btnSave = document.getElementById('btnSave');
    const addQuestionForm = document.getElementById('addQuestionForm');
    const btnCreateNewQuiz = document.getElementById('btnCreateNewQuiz');
    const btnNext = document.getElementById('btnNext');
    const quizInfoForm = document.getElementById('quizInfoForm');

    // Get user info from token
    const user = getCurrentUser();
    const authorId = user ? user.Id : null;

    // Quiz data object
    let quizData = {
        quizId: 0,
        title: '',
        description: '',
        authorId: parseInt(authorId, 10),
        questions: []
    };

    // Answer inputs
    const answers = [
        {
            text: document.getElementById('answer1'),
            correct: document.getElementById('chbxAnswer1')
        },
        {
            text: document.getElementById('answer2'),
            correct: document.getElementById('chbxAnswer2')
        },
        {
            text: document.getElementById('answer3'),
            correct: document.getElementById('chbxAnswer3')
        },
        {
            text: document.getElementById('answer4'),
            correct: document.getElementById('chbxAnswer4')
        }
    ];

    // Update save button state based on question count
    function updateSaveBtnState() {
        const questionCount = quizData.questions.length;
        btnSave.disabled = questionCount < 3 || questionCount > 20;
    }

    // Clear form
    function clearForm() {
        quizName.value = '';
        quizDescription.value = '';
        quizData.questions = [];
        addQuestionForm.reset();
        updateSaveBtnState();
    }

    // Validate question inputs
    function validateQuestion(questionTime, questionMaxPoints) {
        if (questionTime < 15 || questionTime >= 60) {
            alert('Question time must be between 15 and 60 seconds!');
            return false;
        }

        if (questionMaxPoints < 1 || questionMaxPoints >= 1000) {
            alert('Max points must be between 1 and 1000!');
            return false;
        }

        return true;
    }

    // Validate answer inputs (must have exactly one correct answer)
    function validateAnswers(answers) {
        const correctAnswers = answers.filter(answer => answer.correct);
        return correctAnswers.length === 1;
    }

    // Display quizzes in the panel
    function displayQuizzes(quizzes) {
        quizPanel.innerHTML = '';

        if (quizzes.length === 0) {
            quizPanel.innerHTML = '<p>No quizzes found. Create your first quiz!</p>';
            return;
        }

        quizzes.forEach(quiz => {
            const quizDiv = document.createElement('div');
            quizDiv.className = 'quiz-item';

            quizDiv.innerHTML = `
        <h3>${quiz.title}</h3>
        <p>${quiz.description}</p>
        <div class="quiz-actions">
          <button class="btn btn-secondary btn-show-question" data-quiz-id="${quiz.quizId}">
            Show Questions
          </button>
          <button class="btn btn-primary btn-play" data-quiz-id="${quiz.quizId}">
            Play Quiz
          </button>
          <button class="btn btn-outline btn-update" data-quiz-id="${quiz.quizId}">
            Update
          </button>
          <button class="btn btn-danger btn-delete" data-quiz-id="${quiz.quizId}">
            Delete
          </button>
        </div>
      `;

            // Add event listeners
            const showQuestionsBtn = quizDiv.querySelector('.btn-show-question');
            const playBtn = quizDiv.querySelector('.btn-play');
            const updateBtn = quizDiv.querySelector('.btn-update');
            const deleteBtn = quizDiv.querySelector('.btn-delete');

            showQuestionsBtn.addEventListener('click', () => {
                fetchQuizDetails(quiz.quizId);
            });

            playBtn.addEventListener('click', () => {
                localStorage.setItem('quizId', quiz.quizId);

                quizAPI.getQuizById(quiz.quizId)
                    .then(quiz => {
                        const questionsLength = quiz.questions.length;
                        localStorage.setItem('questionsLength', questionsLength);
                        window.location.href = '/pages/game/play.html';
                    })
                    .catch(error => {
                        console.error('Error:', error);
                    });
            });

            updateBtn.addEventListener('click', () => {
                handleUpdateQuiz(quiz);
            });

            deleteBtn.addEventListener('click', () => {
                handleDeleteQuiz(quiz.quizId);
            });

            quizPanel.appendChild(quizDiv);
        });
    }

    // Display quiz questions
    function displayQuizQuestions(quiz) {
        questionPanel.innerHTML = '';

        const questionsTitle = document.createElement('h3');
        questionsTitle.textContent = 'Questions';
        questionPanel.appendChild(questionsTitle);

        quiz.questions.forEach((question, index) => {
            const questionDiv = document.createElement('div');
            questionDiv.className = 'question-item';

            questionDiv.innerHTML = `
        <h4>${index + 1}. ${question.questionText}</h4>
        <p>Time: ${question.questionTime} sec | Points: ${question.questionMaxPoints}</p>
        <button class="btn btn-outline btn-sm btn-update-question" data-question-id="${question.questionId}">
          Update
        </button>
      `;

            const updateQuestionBtn = questionDiv.querySelector('.btn-update-question');
            updateQuestionBtn.addEventListener('click', () => {
                handleUpdateQuestion(quiz.quizId, question);
            });

            questionPanel.appendChild(questionDiv);
        });
    }

    // Handle update quiz
    function handleUpdateQuiz(quiz) {
        const updatedTitle = prompt('Enter new title:', quiz.title);
        const updatedDescription = prompt('Enter new description:', quiz.description);

        if (!updatedTitle || !updatedDescription) {
            alert('Title and description are required.');
            return;
        }

        const updateQuizData = {
            quizId: quiz.quizId,
            title: updatedTitle,
            description: updatedDescription,
            authorId: quiz.authorId,
            questions: quiz.questions
        };

        quizAPI.updateQuiz(quiz.quizId, updateQuizData)
            .then(data => {
                alert('Quiz updated successfully!');
                fetchQuizzes();
            })
            .catch(error => {
                alert('Error updating quiz. Please try again.');
            });
    }

    // Handle update question
    function handleUpdateQuestion(quizId, question) {
        const updatedQuestionText = prompt('Enter new question text:', question.questionText);
        const updatedQuestionTime = prompt('Enter new question time (15-60 seconds):', question.questionTime);
        const updatedQuestionMaxPoints = prompt('Enter new max points (1-1000):', question.questionMaxPoints);

        if (!updatedQuestionText || !updatedQuestionTime || !updatedQuestionMaxPoints) {
            alert('Question text, time, and max points are required.');
            return;
        }

        const updateQuestionData = {
            questionId: question.questionId,
            questionText: updatedQuestionText,
            questionTime: parseInt(updatedQuestionTime, 10),
            questionMaxPoints: parseInt(updatedQuestionMaxPoints, 10),
            quizId: quizId,
            answers: question.answers
        };

        quizAPI.updateQuestion(question.questionId, updateQuestionData)
            .then(data => {
                alert('Question updated successfully!');
                fetchQuizDetails(quizId);
            })
            .catch(error => {
                alert('Error updating question. Please try again.');
            });
    }

    // Handle delete quiz
    function handleDeleteQuiz(quizId) {
        if (confirm('Are you sure you want to delete this quiz?')) {
            quizAPI.deleteQuiz(quizId)
                .then(() => {
                    alert('Quiz deleted successfully!');
                    fetchQuizzes();
                })
                .catch(error => {
                    alert('Error deleting quiz. Please try again.');
                });
        }
    }

    // Fetch quizzes
    function fetchQuizzes() {
        quizAPI.getAllQuizzes()
            .then(data => {
                displayQuizzes(data);
            })
            .catch(error => {
                console.error('Error fetching quizzes:', error);
            });
    }

    // Fetch quiz details
    function fetchQuizDetails(quizId) {
        quizAPI.getQuizById(quizId)
            .then(quiz => {
                displayQuizQuestions(quiz);
            })
            .catch(error => {
                console.error('Error fetching quiz details:', error);
            });
    }

    // Event Listeners

    // Show quiz info form when "Create New Quiz" button is clicked
    if (btnCreateNewQuiz) {
        btnCreateNewQuiz.addEventListener('click', () => {
            if (quizInfoForm) {
                quizInfoForm.style.display = 'block';
                btnCreateNewQuiz.style.display = 'none';

                const quizPanelTitle = document.querySelector('#quizList h2');
                if (quizPanelTitle) {
                    quizPanelTitle.style.display = 'none';
                }
            }
        });
    }

    // Show add question form when "Next" button is clicked
    if (btnNext) {
        btnNext.addEventListener('click', () => {
            if (!quizName.value.trim() || !quizDescription.value.trim()) {
                alert('Please enter quiz name and description');
                return;
            }

            quizData.title = quizName.value;
            quizData.description = quizDescription.value;
            quizData.authorId = parseInt(authorId, 10);

            if (quizInfoForm) quizInfoForm.style.display = 'none';
            if (addQuestionForm) addQuestionForm.style.display = 'block';
        });
    }

    // Add question button click handler
    if (btnAddAnotherQuestion) {
        btnAddAnotherQuestion.addEventListener('click', () => {
            const questionText = document.getElementById('questionText').value;
            const questionTime = parseInt(document.getElementById('questionTime').value, 10);
            const questionMaxPoints = parseInt(document.getElementById('questionMaxPoints').value, 10);

            const questionAnswers = answers.map(answer => ({
                answerText: answer.text.value,
                correct: answer.correct.checked
            }));

            if (!validateAnswers(questionAnswers)) {
                alert('You must select just one correct answer for each question!');
                return;
            }

            if (!validateQuestion(questionTime, questionMaxPoints)) {
                return;
            }

            quizData.questions.push({
                questionId: 0,
                questionText: questionText,
                questionPosition: quizData.questions.length + 1,
                questionTime: questionTime,
                questionMaxPoints: questionMaxPoints,
                answers: questionAnswers
            });

            addQuestionForm.reset();
            console.log('Question added successfully!');
            updateSaveBtnState();

            if (quizData.questions.length >= 3) {
                alert('You can now save the quiz!');
            }
        });
    }

    // Save quiz button click handler
    if (btnSave) {
        btnSave.addEventListener('click', () => {
            if (quizData.questions.length < 3 || quizData.questions.length > 20) {
                alert('Quiz must have at least 3 and at most 20 questions!');
                return;
            }

            quizAPI.createQuiz(quizData)
                .then(data => {
                    alert('Quiz saved successfully!');
                    clearForm();

                    // Hide the question form and show the quiz list
                    if (addQuestionForm) addQuestionForm.style.display = 'none';

                    const quizPanelTitle = document.querySelector('#quizList h2');
                    if (quizPanelTitle) {
                        quizPanelTitle.style.display = 'block';
                    }

                    if (btnCreateNewQuiz) btnCreateNewQuiz.style.display = 'block';

                    fetchQuizzes();
                })
                .catch(error => {
                    alert('Error saving quiz. Please try again.');
                });
        });
    }

    // Initialize the page
    fetchQuizzes();
});
