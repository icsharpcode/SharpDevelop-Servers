@echo off
cd hook
del *.* /F /Q
cd ..
copy source\bin\%1\*.exe hook\
copy source\bin\%1\*.dll hook\
copy source\bin\%1\*.pdb hook\
copy source\bin\%1\*.config hook\
copy source\bin\%1\*.css hook\
cd hook
rename svnpostcommithook.* post-commit.*
cd ..

pause