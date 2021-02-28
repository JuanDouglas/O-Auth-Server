CREATE TABLE [Account](
[ID] INTEGER IDENTITY PRIMARY KEY NOT NULL,
[Key] VARCHAR(300) UNIQUE NOT NULL,
[UserName] VARCHAR(500) UNIQUE NOT NULL,
[Password] VARCHAR(100) NOT NULL
);

CREATE TABLE [Application](
[ID] INTEGER IDENTITY PRIMARY KEY NOT NULL, 
[Key] VARCHAR(300) UNIQUE NOT NULL,
[Redirect URL] VARCHAR(500) NOT NULL,
[Site] VARCHAR(100) NOT NULL,
);

CREATE TABLE [Authorization](
[ID] INTEGER IDENTITY PRIMARY KEY NOT NULL, 
[Account] INTEGER NOT NULL,
[Application] INTEGER NOT NULL,
[Level] INT NOT NULL,
[Date] DATETIME NOT NULL, 
FOREIGN KEY (Account) REFERENCES [Account]([ID]),
FOREIGN KEY (Application) REFERENCES [Application]([ID])
);