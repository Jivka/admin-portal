(function () {
    // Global flag stored on window to persist across DOM mutations
    window._swaggerLoginState = window._swaggerLoginState || { isLoggedIn: false };

    const overrider = () => {
        const swagger = window.ui;
        if (!swagger) {
            console.error('Swagger wasn\'t found');
            return;
        }

        ensureAuthorization(swagger);
        reloadSchemaOnAuth(swagger);
        clearInputPlaceHolder(swagger);
        showLoginUI(swagger);
    }

    const getAuthorization = (swagger) => swagger.auth()._root.entries.find(e => e[0] === 'authorized');
    const isAuthorized = (swagger) => {
        const auth = getAuthorization(swagger);
        return auth && auth[1].size !== 0;
    };

    // Ensure credentials (cookies) are sent with every request
    const ensureAuthorization = (swagger) => {
        // override fetch function of Swagger to make sure cookies are included
        const fetch = swagger.fn.fetch;
        swagger.fn.fetch = (req) => {
            // Ensure credentials are sent with requests to include cookies
            req.credentials = 'include';
            return fetch(req);
        }
    };
    // makes that once user triggers performs authorization,
    // the schema will be reloaded from backend url
    const reloadSchemaOnAuth = (swagger) => {
        const getCurrentUrl = () => {
            const spec = swagger.getState()._root.entries.find(e => e[0] === 'spec');
            if (!spec)
                return undefined;

            const url = spec[1]._root.entries.find(e => e[0] === 'url');
            if (!url)
                return undefined;

            return url[1];
        }
        const reload = () => {
            const url = getCurrentUrl();
            if (url) {
                swagger.specActions.download(url);
            }
        };

        const handler = (caller, args) => {
            const result = caller(args);
            if (result.then) {
                result.then(() => reload())
            }
            else {
                reload();
            }
            return result;
        }

        const auth = swagger.authActions.authorize;
        swagger.authActions.authorize = (args) => handler(auth, args);
        const logout = swagger.authActions.logout;
        swagger.authActions.logout = (args) => handler(logout, args);
    };
    /**
     * Reset input element placeholder
     * @param {any} swagger
     */
    const clearInputPlaceHolder = (swagger) => {
        //https://github.com/api-platform/core/blob/main/src/Bridge/Symfony/Bundle/Resources/public/init-swagger-ui.js#L6-L41
        new MutationObserver(function (mutations, self) {
            let elements = document.querySelectorAll("input[type=text]");
            for (const element of elements)
                element.placeholder = "";
        }).observe(document, { childList: true, subtree: true });
    }
    /**
     * Show login UI
     * @param {any} swagger
     */
    const showLoginUI = (swagger) => {
        //https://github.com/api-platform/core/blob/main/src/Bridge/Symfony/Bundle/Resources/public/init-swagger-ui.js#L6-L41
        let observer = new MutationObserver(function (mutations, self) {
            // Check global flag - if already logged in, don't show login UI
            if (window._swaggerLoginState.isLoggedIn)
                return;

            let rootDiv = document.querySelector("#swagger-ui > section > div.swagger-ui > div:nth-child(2)");
            if (rootDiv == null)
                return;

            let informationContainerDiv = rootDiv.querySelector("div.information-container.wrapper");
            if (informationContainerDiv == null)
                return;

            let descriptionDiv = informationContainerDiv.querySelector("section > div > div > div.description");
            if (descriptionDiv == null)
                return;

            let loginDiv = descriptionDiv.querySelector("div.login");
            if (loginDiv != null)
                return;

            //Check authentication
            if (isAuthorized(swagger))
                return;

            //Remove elements different from information-container wrapper
            for (const element of rootDiv.children) {
                let child = element;
                if (child !== informationContainerDiv)
                    child.remove();
            }

            //Create UI di login
            createLoginUI(descriptionDiv);

        });
        observer.observe(document, { childList: true, subtree: true });

        /**
         * Create login ui elements
         * @param {any} rootDiv
         */
        const createLoginUI = function (rootDiv) {
            let div = document.createElement("div");
            div.className = "login";

            rootDiv.appendChild(div);

            //UserName
            let userNameLabel = document.createElement("label");
            div.appendChild(userNameLabel);

            let userNameSpan = document.createElement("span");
            userNameSpan.innerText = "User";
            userNameLabel.appendChild(userNameSpan);

            let userNameInput = document.createElement("input");
            userNameInput.type = "text";
            userNameInput.style = "margin-left: 10px; margin-right: 10px;";
            userNameLabel.appendChild(userNameInput);

            //Password
            let passwordLabel = document.createElement("label");
            div.appendChild(passwordLabel);

            let passwordSpan = document.createElement("span");
            passwordSpan.innerText = "Password";
            passwordLabel.appendChild(passwordSpan);

            let passwordInput = document.createElement("input");
            passwordInput.type = "password";
            passwordInput.style = "margin-left: 10px; margin-right: 10px;";
            passwordLabel.appendChild(passwordInput);

            //Login button
            let loginButton = document.createElement("button")
            loginButton.type = "submit";
            loginButton.type = "button";
            loginButton.classList.add("btn");
            loginButton.classList.add("auth");
            loginButton.classList.add("authorize");
            loginButton.classList.add("button");
            loginButton.innerText = "Login";
            loginButton.onclick = function () {
                let userName = userNameInput.value;
                let password = passwordInput.value;

                if (userName === "" || password === "") {
                    alert("Input username and password!");
                    return;
                }

                login(userName, password);
            };

            div.appendChild(loginButton);
        }
        /**
         * Manage login
         * @param {any} userName UserName
         * @param {any} password Password
         */
        const login = function (userName, password) {
            let xhr = new XMLHttpRequest();

            xhr.onreadystatechange = function () {
                if (xhr.readyState == XMLHttpRequest.DONE) {
                    if (xhr.status == 200 /* || xhr.status == 400 */) {

                        let response = JSON.parse(xhr.responseText);

                        // Set the GLOBAL flag to prevent login UI from showing again
                        // This persists across DOM mutations and schema reloads
                        window._swaggerLoginState.isLoggedIn = true;

                        // Remove the login div from DOM
                        let loginDiv = document.querySelector("div.login");
                        if (loginDiv) {
                            loginDiv.remove();
                        }

                        // Disconnect the observer to prevent it from re-showing login UI
                        observer.disconnect();

                        // Server now uses session-based authentication
                        // SessionId cookie is automatically sent with all requests
                        // The server retrieves JWT from session on each request
                        
                        let obj = {
                            "Bearer": {
                                "name": "Bearer",
                                "schema": {
                                    "type": "apiKey",
                                    "description": "Authentication via SessionId cookie (automatically included)",
                                    "name": "Authorization",
                                    "in": "header"
                                },
                                value: "Session-based (SessionId cookie)"
                            }
                        };

                        swagger.authActions.authorize(obj);
                        
                        // Add logout button
                        addLogoutButton();
                    }
                    else {
                        alert('Error: ' + xhr.status + ' ' + xhr.statusText + ' ' + xhr.responseText);
                    }
                }
            };

            xhr.open("POST", "/identity/sign-in", true);
            xhr.setRequestHeader("Content-Type", "application/json");
            xhr.withCredentials = true;

            let json = JSON.stringify({ "Email": userName, "Password": password });

            xhr.send(json);
        }
        
        /**
         * Add logout button to Swagger UI
         */
        const addLogoutButton = function () {
            // Wait a bit for Swagger UI to fully render
            setTimeout(() => {
                // Find the information container
                let informationContainerDiv = document.querySelector("div.information-container.wrapper");
                if (!informationContainerDiv)
                    return;

                let descriptionDiv = informationContainerDiv.querySelector("section > div > div > div.description");
                if (!descriptionDiv)
                    return;

                // Check if logout button already exists
                if (descriptionDiv.querySelector("div.logout-container"))
                    return;

                // Create logout container
                let logoutDiv = document.createElement("div");
                logoutDiv.className = "logout-container";
                logoutDiv.style = "margin-top: 20px; padding: 10px; border-top: 1px solid #ccc;";

                // Create logout button
                let logoutButton = document.createElement("button");
                logoutButton.type = "button";
                logoutButton.classList.add("btn");
                logoutButton.classList.add("auth");
                logoutButton.classList.add("authorize");
                logoutButton.classList.add("button");
                logoutButton.innerText = "Logout";
                logoutButton.style = "background-color: #f44336; color: white;";
                
                logoutButton.onclick = function () {
                    logout();
                };

                logoutDiv.appendChild(logoutButton);
                descriptionDiv.appendChild(logoutDiv);
            }, 500);
        }

        /**
         * Handle logout
         */
        const logout = function () {
            let xhr = new XMLHttpRequest();

            xhr.onreadystatechange = function () {
                if (xhr.readyState == XMLHttpRequest.DONE) {
                    if (xhr.status == 200) {
                        // Reset login state
                        window._swaggerLoginState.isLoggedIn = false;
                        
                        // Clear Swagger authorization
                        swagger.authActions.logout();
                        
                        // Reload the page to show login UI again
                        window.location.reload();
                    }
                    else {
                        alert('Logout failed: ' + xhr.status + ' ' + xhr.statusText);
                    }
                }
            };

            xhr.open("POST", "/identity/logout", true);
            xhr.setRequestHeader("Content-Type", "application/json");
            xhr.withCredentials = true;

            xhr.send();
        }
    }

    // append to event right after SwaggerUIBundle initialized
    window.addEventListener('load', () => setTimeout(overrider, 0), false);
}());
