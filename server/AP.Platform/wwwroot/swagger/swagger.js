(function () {
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
        new MutationObserver(function (mutations, self) {
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

        }).observe(document, { childList: true, subtree: true });

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

                        // Get the JWT token from the cookie set by the server
                        // The server now sets cookies, so we need to read from them
                        // For Swagger, we'll need to set up authorization with a placeholder
                        // since cookies are automatically sent with requests
                        
                        let obj = {
                            "Bearer": {
                                "name": "Bearer",
                                "schema": {
                                    "type": "apiKey",
                                    "description": "Authentication via cookies (automatically included)",
                                    "name": "Authorization",
                                    "in": "header"
                                },
                                value: "Bearer (cookie-based)"
                            }
                        };

                        swagger.authActions.authorize(obj);
                        
                        // Reload the page to ensure cookies are properly set
                        setTimeout(() => window.location.reload(), 500);
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
    }

    // append to event right after SwaggerUIBundle initialized
    window.addEventListener('load', () => setTimeout(overrider, 0), false);
}());