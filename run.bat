@echo off
echo تشغيل نظام إدارة متجر الملابس...
echo =====================================
echo.

REM التحقق من وجود .NET 6.0
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo خطأ: .NET 6.0 غير مثبت على النظام
    echo يرجى تحميل وتثبيت .NET 6.0 Runtime من:
    echo https://dotnet.microsoft.com/download/dotnet/6.0
    pause
    exit /b 1
)

REM بناء المشروع
echo جاري بناء المشروع...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo خطأ في بناء المشروع
    pause
    exit /b 1
)

REM تشغيل التطبيق
echo بدء تشغيل البرنامج...
dotnet run --configuration Release
pause