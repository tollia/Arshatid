CREATE  TABLE TolliTest.dbo.Arshatid ( 
	Pk                   int    IDENTITY ( 1 , 1 )  NOT NULL,
	Year                 int      NOT NULL,
	Heading              nvarchar(255)      NOT NULL,
	Description          nvarchar(max)      NOT NULL,
	SendDescription      nvarchar(max)      NULL,
	RegistrationStartTime datetime2      NOT NULL,
	RegistrationEndTime  datetime2      NOT NULL,
	CONSTRAINT pk_Arshatid PRIMARY KEY  ( Pk ) 
 );

