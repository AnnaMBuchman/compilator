cd ..\..
..\Gppg.exe /gplex /conflicts /report /verbose parser.y > parser.cs
..\gplex.exe lekser.lex
:: sleep 300
 
