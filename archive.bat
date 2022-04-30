@echo off

setlocal EnableDelayedExpansion


echo �r���h�����ނ�I�����Ă�������
echo release:0 early:1
set /P BUILD_TYPE="�ǂ�Ńr���h���܂����H: "


if "%BUILD_TYPE%" equ "0" (
    set APP_RELEASE_DIR=".\\source\\AviUtlAutoInstaller\\bin\\Release"
    set UPDATER_RELEASE_DIR=".\\source\\Updater\\bin\\Release"
    set APP_EXE=aai.exe
    set OUTPUT_DIR=".\\release"
) else if "%BUILD_TYPE%" equ "1" (
    set EARLY_VERSION_FILE=".\\source\\AviUtlAutoInstaller\\Models\\ProductInfo.cs"
    set APP_RELEASE_DIR=".\\source\\AviUtlAutoInstaller\\bin\\Early"
    set APP_EXE=aai-early.exe
    set OUTPUT_DIR=".\\early"
) else (
    echo �I��������ނ��Ԉ���Ă��܂�
    pause
    exit
)

set APP_NAME=AviUtlAutoInstaller
set APP_VERSION=
set APP_EARLY_VERSION=
set VERSION_FILE=".\\source\\AviUtlAutoInstaller\\Properties\\AssemblyInfo.cs"
set MANUAL_DIR=".\\docs"
set LICENSE_DIR=".\\Licenses"

@rem ��{�Œ�
set SV_EXE="""C:\Program Files\7-Zip\7z.exe"""

@rem release�f�B���N�g�������݂��邩�`�F�b�N
if not exist "%APP_RELEASE_DIR%\\%APP_EXE%" (
    echo �A�v���P�[�V�������r���h���Ă�������
    pause
    exit
)

if "%BUILD_TYPE%" equ "0" (
    @rem release�f�B���N�g�������݂��邩�`�F�b�N
    if not exist "%UPDATER_RELEASE_DIR%\\updater.exe" (
        echo �A�b�v�f�[�^�[���r���h���Ă�������
        pause
        exit
    )
)

@rem �o�[�W�����擾
findstr /I /R "[assembly: AssemblyVersion" "%VERSION_FILE%" | findstr /I /R /V "\/\/" > "archive_ver.txt"

for /f "usebackq tokens=2 delims=()" %%i in ("archive_ver.txt") do (
    set APP_VERSION=%%~i
)
call :STRLEN %APP_VERSION%
set VER_LEN=%ERRORLEVEL%
set /a VER_LEN2=%VER_LEN%-2
call set APP_VERSION=%%APP_VERSION:~0,%VER_LEN2%%%


if "%BUILD_TYPE%" equ "1" (
    findstr /I /R "EralyVersion" "%EARLY_VERSION_FILE%" > "archive_ver.txt"

    for /f "usebackq tokens=6 delims= " %%i in ("archive_ver.txt") do (
        set APP_EARLY_VERSION=%%~i
    )
    set APP_EARLY_VERSION=!APP_EARLY_VERSION:~0,-2!

    call set APP_VERSION=%APP_VERSION%-Early!APP_EARLY_VERSION!
)


echo %APP_VERSION%
del "archive_ver.txt"

set ARCHIVE_FILE=%APP_NAME%_v%APP_VERSION%.zip

@rem �t�@�C��/�f�B���N�g���R�s�[
mkdir %OUTPUT_DIR%
xcopy /e "%APP_RELEASE_DIR%" %OUTPUT_DIR%
del "%OUTPUT_DIR%\\*.pdb"
rmdir /s /q "%OUTPUT_DIR%\\cache"
mkdir %OUTPUT_DIR%\\manual
xcopy /s %MANUAL_DIR% "%OUTPUT_DIR%\\manual"
rmdir /s /q "%OUTPUT_DIR%\\manual\\images\\base"
mkdir %OUTPUT_DIR%\\Licenses
xcopy /s %LICENSE_DIR% "%OUTPUT_DIR%\\Licenses"
copy ".\\LICENSE" "%OUTPUT_DIR%"
if "%BUILD_TYPE%" equ "0" (
    copy "%UPDATER_RELEASE_DIR%\\updater.*" %OUTPUT_DIR%
)

@rem ���k
%SV_EXE% a -tzip "%ARCHIVE_FILE%" "%OUTPUT_DIR%\\*"
rmdir /s /q "%OUTPUT_DIR%"

exit



@rem �ȉ��A�T�u���[�`��

@rem ������̒�����Ԃ�
@rem ����: %1-�����Ώ�
@rem �߂�l 0<=:������ -1:%1����
:STRLEN
    if "%~1" equ "" exit /b -1
    set str=%~1
    set len=0
:STRLEN_LOOP
    if "%str%" equ "" exit /b %len%
    set str=%str:~1%
    set /a len+=1
    goto :STRLEN_LOOP
