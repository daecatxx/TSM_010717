
SET IDENTITY_INSERT [dbo].[CustomerAccount] ON 
INSERT [dbo].[CustomerAccount] ([CustomerAccountId], [AccountName], [Comments], [IsVisible], [CreatedById], [CreatedAt], [UpdatedById], [UpdatedAt]) VALUES (5, N'SCHOOL A', N'Weekly one lesson (Saturday) on dancing foundations (3 hours each week).', 1, 1, CAST(N'2017-01-01 15:40:15.5030000' AS DateTime2), 1, CAST(N'2017-01-01 15:40:15.5030000' AS DateTime2))
INSERT [dbo].[CustomerAccount] ([CustomerAccountId], [AccountName], [Comments], [IsVisible], [CreatedById], [CreatedAt], [UpdatedById], [UpdatedAt]) VALUES (6, N'SCHOOL B', N'Weekly two lessons (total 5 hours) dancing gym sessions', 1, 2, CAST(N'2017-03-20 15:40:15.5030000' AS DateTime2), 2, CAST(N'2017-09-15 15:40:15.5030000' AS DateTime2))
INSERT [dbo].[CustomerAccount] ([CustomerAccountId], [AccountName], [Comments], [IsVisible], [CreatedById], [CreatedAt], [UpdatedById], [UpdatedAt]) VALUES (7, N'SCHOOL C', N'Weekly two lessons (total 4 hours) dancing lessons', 1, 2, CAST(N'2017-04-14 15:40:15.5030000' AS DateTime2), 2, CAST(N'2017-05-18 12:36:23.1488233' AS DateTime2))
INSERT [dbo].[CustomerAccount] ([CustomerAccountId], [AccountName], [Comments], [IsVisible], [CreatedById], [CreatedAt], [UpdatedById], [UpdatedAt]) VALUES (8, N'SCHOOL D', N'Many lessons to be conducted every week day (MON to SAT)', 1, 2, CAST(N'2017-04-01 15:40:15.5030000' AS DateTime2), 2, CAST(N'2017-05-18 15:01:24.8665642' AS DateTime2))
SET IDENTITY_INSERT [dbo].[CustomerAccount] OFF

SET IDENTITY_INSERT [dbo].[SessionSynopsis] ON 
INSERT INTO SessionSynopsis
                         (SessionSynopsisId,SessionSynopsisName,  IsVisible, CreatedById, UpdatedById)
VALUES        (1,'Training' ,1,2,2)
INSERT INTO SessionSynopsis
                         (SessionSynopsisId,SessionSynopsisName, IsVisible, CreatedById, UpdatedById)
VALUES        (2,'Training & Rehearsal' ,1,2,2)
INSERT INTO SessionSynopsis
                         (SessionSynopsisId,SessionSynopsisName,  IsVisible, CreatedById, UpdatedById)
VALUES        (3,'Rehearsal' ,1,2,2)
INSERT INTO SessionSynopsis
                         (SessionSynopsisId,SessionSynopsisName, IsVisible, CreatedById, UpdatedById)
VALUES        (4,'Rehearsal & performance' ,1,2,2)
INSERT INTO SessionSynopsis
                         (SessionSynopsisId,SessionSynopsisName, IsVisible, CreatedById, UpdatedById)
VALUES        (5,'Performance' ,1,2,2)
SET IDENTITY_INSERT [dbo].[SessionSynopsis] OFF 

SET IDENTITY_INSERT [dbo].[AccountDetail] ON 
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (3, 6, 1, CAST(N'2017-06-01 00:00:00.0000000' AS DateTime2), NULL, 600, 720, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (5, 6, 4, CAST(N'2017-06-01 00:00:00.0000000' AS DateTime2), NULL, 540, 780, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (6, 7, 2, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 720, 840, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (8, 7, 2, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 660, 780, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (9, 8, 1, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 900, 1020, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (10, 8, 1, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 900, 1140, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (11, 8, 2, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 540, 660, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (12, 8, 3, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 540, 660, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (13, 8, 4, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 540, 660, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (14, 8, 5, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 540, 660, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (15, 8, 6, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 540, 660, 1)
INSERT [dbo].[AccountDetail] ([AccountDetailId], [CustomerAccountId], [DayOfWeekNumber], [EffectiveStartDate], [EffectiveEndDate], [StartTimeInMinutes], [EndTimeInMinutes], [IsVisible]) VALUES (16, 8, 7, CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL, 540, 720, 1)
SET IDENTITY_INSERT [dbo].[AccountDetail] OFF

SET IDENTITY_INSERT [dbo].[AccountRate] ON 
INSERT [dbo].[AccountRate] ([AccountRateId], [CustomerAccountId], [RatePerHour], [EffectiveStartDate], [EffectiveEndDate]) VALUES (1, 6, CAST(150.00 AS Decimal(6, 2)), CAST(N'2017-06-01 00:00:00.0000000' AS DateTime2), NULL)
INSERT [dbo].[AccountRate] ([AccountRateId], [CustomerAccountId], [RatePerHour], [EffectiveStartDate], [EffectiveEndDate]) VALUES (2, 6, CAST(200.00 AS Decimal(6, 2)), CAST(N'2018-03-01 00:00:00.0000000' AS DateTime2), NULL)
INSERT [dbo].[AccountRate] ([AccountRateId], [CustomerAccountId], [RatePerHour], [EffectiveStartDate], [EffectiveEndDate]) VALUES (3, 5, CAST(100.00 AS Decimal(6, 2)), CAST(N'2016-03-01 00:00:00.0000000' AS DateTime2), NULL)
INSERT [dbo].[AccountRate] ([AccountRateId], [CustomerAccountId], [RatePerHour], [EffectiveStartDate], [EffectiveEndDate]) VALUES (4, 7, CAST(120.00 AS Decimal(6, 2)), CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL)
INSERT [dbo].[AccountRate] ([AccountRateId], [CustomerAccountId], [RatePerHour], [EffectiveStartDate], [EffectiveEndDate]) VALUES (5, 8, CAST(100.00 AS Decimal(6, 2)), CAST(N'2017-05-01 00:00:00.0000000' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[AccountRate] OFF