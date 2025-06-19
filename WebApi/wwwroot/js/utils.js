/**
 * QuizCraft Utility Functions
 */

// Check if user is authenticated
function isAuthenticated() {
    return localStorage.getItem('token') !== null;
}

// Redirect to login if not authenticated
function requireAuth() {
    if (!isAuthenticated()) {
        alert('Please log in to access this page');
        window.location.href = '/pages/auth/login.html';
        return false;
    }
    return true;
}

// Get JWT token
function getToken() {
    return localStorage.getItem('token');
}

// Decode JWT token
function decodeToken(token) {
    try {
        // Simple JWT decode for payload
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        return JSON.parse(jsonPayload);
    } catch (error) {
        console.error('Error decoding token:', error);
        return null;
    }
}

// Get current user info
function getCurrentUser() {
    const token = getToken();
    if (!token) return null;

    return decodeToken(token);
}

// Display username in UI
function displayUsername() {
    const userNameDisplay = document.getElementById('userNameDisplay');
    if (!userNameDisplay) return;

    const user = getCurrentUser();
    if (user && user.Username) {
        userNameDisplay.textContent = user.Username;
    } else {
        userNameDisplay.textContent = 'User';
    }
}

// Format date
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Add event listener for logout buttons
function setupLogoutButton() {
    const logoutBtn = document.getElementById('logoutBtn');
    if (!logoutBtn) return;

    logoutBtn.addEventListener('click', (e) => {
        e.preventDefault();

        // Clear auth data
        localStorage.removeItem('token');
        localStorage.removeItem('username');
        localStorage.removeItem('authorId');

        console.log('User logged out, tokens cleared');
        window.location.href = '/pages/auth/login.html';
    });
}

// Initialize common UI elements
function initUI() {
    if (isAuthenticated()) {
        displayUsername();
        setupLogoutButton();
    }
}

// Document ready function
function onDocumentReady(fn) {
    if (document.readyState !== 'loading') {
        fn();
    } else {
        document.addEventListener('DOMContentLoaded', fn);
    }
}

// Initialize app
onDocumentReady(() => {
    initUI();
});
