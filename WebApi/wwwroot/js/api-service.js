const API_BASE_URL = '';  // Base URL is empty for same-origin requests

// Standard fetch with auth token and JSON handling
async function apiFetch(endpoint, options = {}) {
    const token = localStorage.getItem('token');

    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` })
        }
    };

    const fetchOptions = {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...(options.headers || {})
        }
    };

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, fetchOptions);

        // Check if response is JSON
        const contentType = response.headers.get('content-type');
        const isJson = contentType && contentType.indexOf('application/json') !== -1;

        // Handle non-JSON responses
        if (!isJson) {
            if (!response.ok) {
                throw new Error(`API error: ${response.status} ${response.statusText}`);
            }
            return response;
        }

        const data = await response.json();

        if (!response.ok) {
            const error = new Error(data.message || `API error: ${response.status} ${response.statusText}`);
            error.status = response.status;
            error.data = data;
            throw error;
        }

        return data;
    } catch (error) {
        console.error('API request failed:', error);
        throw error;
    }
}

// Auth API
const authAPI = {
    login: async (username, password) => {
        return apiFetch('/api/User/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
    },

    register: async (userData) => {
        return apiFetch('/api/User/register', {
            method: 'POST',
            body: JSON.stringify(userData)
        });
    },

    updateProfile: async (userData) => {
        return apiFetch('/api/User', {
            method: 'PUT',
            body: JSON.stringify(userData)
        });
    },

    changePassword: async (passwordData) => {
        return apiFetch('/api/User/changePassword', {
            method: 'PUT',
            body: JSON.stringify(passwordData)
        });
    },
    getProfile: function (userId) {
        return apiFetch(`/api/user/profile/${userId}`);
    }

};

// Quiz API
const quizAPI = {
    getAllQuizzes: async () => {
        return apiFetch('/api/quiz');
    },

    getQuizById: async (quizId) => {
        return apiFetch(`/api/quiz/${quizId}`);
    },

    getQuizzesByAuthor: async (authorId) => {
        return apiFetch(`/api/quiz/details/${authorId}`);
    },

    createQuiz: async (quizData) => {
        return apiFetch('/api/quiz', {
            method: 'POST',
            body: JSON.stringify(quizData)
        });
    },

    updateQuiz: async (quizId, quizData) => {
        console.log(`Sending update request for quiz ${quizId} with data:`, JSON.stringify(quizData));
        const response = await apiFetch(`/api/quiz/${quizId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(quizData)
        });
        console.log("API response:", response);
        return response;
    },

    deleteQuiz: async (quizId) => {
        return apiFetch(`/api/quiz/${quizId}`, {
            method: 'DELETE'
        });
    },
    updateQuestion: async (questionId, questionData) => {
        try {
            // Ensure all required fields have values before sending
            const sanitizedData = {
                questionId: questionData.questionId || 0,
                questionText: questionData.questionText || "New Question",
                questionPosition: questionData.questionPosition || 1,
                questionTime: questionData.questionTime || 30,
                questionMaxPoints: questionData.questionMaxPoints || 100,
                quizId: questionData.quizId,
                answers: (questionData.answers || []).map((answer, index) => ({
                    answerId: answer.answerId || 0,
                    answerText: answer.answerText || `Answer ${index + 1}`,
                    correct: !!answer.correct
                }))
            };

            console.log("Sending question data:", JSON.stringify(sanitizedData));

            const endpoint = sanitizedData.questionId === 0 ?
                '/api/question' :
                `/api/question/${sanitizedData.questionId}`;

            const method = sanitizedData.questionId === 0 ? 'POST' : 'PUT';

            return await apiFetch(endpoint, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(sanitizedData)
            });
        } catch (error) {
            console.error("Error in updateQuestion:", error);
            throw error;
        }
    },
    getQuestion: async (questionId) => {
        return apiFetch(`/api/question/${questionId}`);
    },
    deleteQuestion: async (questionId) => {
        return apiFetch(`/api/question/${questionId}`, {
            method: 'DELETE'
        });
    }
};

// Quiz History API
const historyAPI = {
    getQuizHistories: async () => {
        return apiFetch('/api/QuizHistory');
    },

    getQuizHistoryByAuthorId: async (authorId) => {
        return apiFetch(`/api/QuizHistory/author/${authorId}`);
    },

    getQuizHistoryByQuizId: async (quizId) => {
        return apiFetch(`/api/QuizHistory/quiz/${quizId}`);
    },

    addQuizHistory: async (historyData) => {
        return apiFetch('/api/QuizHistory', {
            method: 'POST',
            body: JSON.stringify(historyData)
        });
    }
};
