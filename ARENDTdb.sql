CREATE TABLE Users(
	ID int primary key identity(1, 1),
	Username nvarchar(75) unique not null,
	PasswordHash nvarchar(256) not null,
	PasswordSalt nvarchar(256) not null,
	FirstName nvarchar(50) not null,
	Email nvarchar(50) not null,
	LastName nvarchar(50) not null,
	JoinDate date not null
);

CREATE TABLE Quizzes (
    ID          INT PRIMARY KEY IDENTITY(1, 1),
    Title       NVARCHAR(75)  UNIQUE      NOT NULL,
    Description NVARCHAR(MAX)            NOT NULL,
    AuthorID    INT           NOT NULL,  
    CONSTRAINT FK_Quizzes_Users
        FOREIGN KEY (AuthorID)
        REFERENCES Users(ID)
        ON DELETE CASCADE
);


CREATE TABLE Questions(
	ID int primary key identity(1, 1),
	QuestionText nvarchar(max) not null,
	QuizID int foreign key references Quizzes(ID) on delete cascade not null,
	QuestionPosition int not null,
	QuestionTime int not null default 30,
	QuestionMaxPoints int not null,
);

CREATE TABLE Answers(
	ID int primary key identity(1, 1),
	QuestionID int foreign key references Questions(ID) on delete cascade not null,
	AnswerText nvarchar(150) not null,
	Correct bit not null default 0
);

CREATE TABLE QuizHistory(
	ID int primary key identity(1, 1),
	QuizID int foreign key references Quizzes(ID) not null,
	WinnerID int foreign key references Users(ID),
	PlayedAt datetime not null,
	WinnerName nvarchar(75) not null,
	WinnerScore int not null
);

CREATE TABLE QuizRecord(
	ID int primary key identity(1, 1),
	QuizID int foreign key references Quizzes(ID) not null,
	SessionId char(6) not null,
	PlayerName nvarchar(75) not null,
	Score int default 0
);