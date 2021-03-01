# O-Auth-Server
Servidor de autenticação OAuth2.0

#Sequence of steps for authorization
- **_1._** Origin Server
	- C# Code for AuthServer Request.
``` C#
HttpClient httpClient = new HttpClient();
await httpClient.SendAsync("https;\\nexus-authentication.duckdns.org\api\OAuth?app_code={your_app_code}&web_view=true");
```
- **_2._** Auth Server 
	- **_2.1._** Get Authorization (_Web View_).
		- **PARAMETER NAME**;**KEY**;**DEFINITION**
		- **Application Key**;**app_key**; Guid (Universal Unique Identifier) ​​used to identify the application that will request the Login.
		- **View Web Page**;**web_view**; It defines if the one who will authenticate is the App or a WEB page.
		- If you are logged in skip to step **1.2**
		- **_2.1._** Login Server
			- **_2.1.1._** First Step (_Web View_).
				- **PARAMETER NAME**;**KEY**;**DEFINITION**
				- **URl Post**;**post**; URL that will be redirected after signing in.
			- **_2.1.2._** First Step
				- **PARAMETER NAME**;**KEY**;**DEFINITION**
				- **URl Post**;**post**; URL that will be redirected after signing in.
				- **It's an APi**; **is_api**; Indicates if the login will be made through the web page of this server or if the values ​​are returned to the system that will consume this API.
				- **UserName**; **user**; Define which user will request the password in the next step	
			- **_2.1.3._** Second Step (_Web View_).
				- **PARAMETER NAME**;**KEY**;**DEFINITION**
				- **URl Post**;**post**; URL that will be redirected after signing in.
				- **It's an APi**; **is_api**; Indicates if the login will be made through the web page of this server or if the values ​​are returned to the system that will consume this API.
				- **First Step Key**; **key**; Key returned by the first step.	 	
			- **_2.1.4._** Second Step 
				- **PARAMETER NAME**;**KEY**;**DEFINITION**
				- **URl Post**;**post**; URL that will be redirected after signing in.
				- **It's an APi**; **is_api**; Indicates if the login will be made through the web page of this server or if the values ​​are returned to the system that will consume this API.
				- **First Step Key**; **key**; Key returned by the first step.		
				- **Password**; **pwd**; Account access password
				- If authentication is successful, the server will redirect to step **2.2**