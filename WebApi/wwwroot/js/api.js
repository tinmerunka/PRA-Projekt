/**
 * Quiz API functions
 */
const quizAPI = {
    // Get all quizzes
    getAllQuizzes: () => {
        return fetchApi('/api/quiz', 'GET');
    },

    // Get quiz by ID (with full details)
    getQuizById: (id) => {
        return fetchApi(`/api/quiz/${id}`, 'GET');
    },

    // Get quiz info (title, description, question count)
    getQuizInfo: (id) => {
        return fetchApi(`/api/quiz/${id}/info`, 'GET');
    },

    // Get a specific question from a quiz
    getQuestion: (quizId, position) => {
        return fetchApi(`/api/quiz/${quizId}/questions/${position}`, 'GET');
    },

    // Start a new quiz attempt
    startQuiz: (playerName, quizId) => {
        return fetchApi('/api/quiz/start', 'POST', {
            playerName: playerName,
            quizId: quizId
        });
    },

    // Submit an answer to a question
    submitAnswer: (quizId, questionId, playerId, answerIndex, timeRemaining) => {
        return fetchApi(`/api/quiz/${quizId}/submit-answer`, 'POST', {
            playerId: playerId,
            questionId: questionId,
            answerIndex: answerIndex,
            timeRemaining: timeRemaining
        });
    },

    // Get quiz results
    getQuizResults: (playerId, quizId) => {
        return fetchApi(`/api/quiz/player/${playerId}/results/${quizId}`, 'GET');
    },

    // Create a new quiz
    createQuiz: (quizData) => {
        return fetchApi('/api/quiz', 'POST', quizData);
    },

    // Update an existing quiz
    updateQuiz: (id, quizData) => {
        return fetchApi(`/api/quiz/${id}`, 'PUT', quizData);
    },

    // Delete a quiz
    deleteQuiz: (id) => {
        return fetchApi(`/api/quiz/${id}`, 'DELETE');
    }
};
