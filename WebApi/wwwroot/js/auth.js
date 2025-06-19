/**
 * Authentication Module
 */
onDocumentReady(() => {
    // Login form handling
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const username = document.getElementById('username').value.trim();
            const password = document.getElementById('password').value.trim();

            if (!username || !password) {
                alert('Please enter username and password.');
                return;
            }

            try {
                const submitBtn = loginForm.querySelector('button[type="submit"]');
                const originalButtonText = submitBtn.textContent;
                submitBtn.textContent = 'Logging in...';
                submitBtn.disabled = true;

                const response = await authAPI.login(username, password);

                if (response && response.token) {
                    // Store token
                    localStorage.setItem('token', response.token);

                    // Decode token and store user info
                    const decodedToken = decodeToken(response.token);
                    if (decodedToken) {
                        console.log("Decoded token:", decodedToken); // Add this line to debug

                        // Check for common username properties in JWT tokens
                        const username = decodedToken.Username ||
                            decodedToken.username ||
                            decodedToken.unique_name ||
                            decodedToken.sub ||
                            username; // Fallback to the entered username

                        localStorage.setItem('username', username);
                        localStorage.setItem('authorId', decodedToken.Id || decodedToken.id || '1');
                    }

                    alert('Login successful!');
                    window.location.href = '/pages/quiz/list.html';
                } else {
                    alert('Login failed. Please check your credentials.');
                }
            } catch (error) {
                console.error('Login error:', error);
                alert(error.message || 'Login failed. Please try again.');
            } finally {
                const submitBtn = loginForm.querySelector('button[type="submit"]');
                submitBtn.textContent = 'Login';
                submitBtn.disabled = false;
            }
        });
    }

    // Register form handling
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const username = document.getElementById('reg-username').value.trim();
            const email = document.getElementById('reg-email').value.trim();
            const password = document.getElementById('reg-password').value.trim();
            const firstName = document.getElementById('reg-firstName').value.trim();
            const lastName = document.getElementById('reg-lastName').value.trim();

            if (!username || !email || !password) {
                alert('Please fill in all required fields.');
                return;
            }

            try {
                const submitBtn = registerForm.querySelector('button[type="submit"]');
                const originalButtonText = submitBtn.textContent;
                submitBtn.textContent = 'Creating account...';
                submitBtn.disabled = true;

                const userData = {
                    username,
                    email,
                    password,
                    firstName,
                    lastName
                };

                const response = await authAPI.register(userData);

                alert('Registration successful! Please log in.');
                window.location.href = '/pages/auth/login.html';
            } catch (error) {
                console.error('Registration error:', error);
                alert(error.message || 'Registration failed. Please try again.');
            } finally {
                const submitBtn = registerForm.querySelector('button[type="submit"]');
                submitBtn.textContent = 'Register';
                submitBtn.disabled = false;
            }
        });
    }
});
