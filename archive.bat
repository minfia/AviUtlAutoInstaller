@echo off

setlocal EnableDelayedExpansion


set APP_NAME=AviUtlAutoInstaller
set APP_VERSION=
set VERSION_FILE=".\\source\\AviUtlAutoInstaller\\Properties\\AssemblyInfo.cs"
set APP_RELEASE_DIR=".\\source\\AviUtlAutoInstaller\\bin\\Release"
set UPDATER_RELEASE_DIR=".\\source\\Updater\\bin\\Release"
set OUTPUT_DIR=".\\release"
set MANUAL_DIR=".\\docs"
set LICENSE_DIR=".\\Licenses"

@rem ��{�Œ�
set SV_EXE="""C:\Program Files\7-Zip\7z.exe"""

@rem release�f�B���N�g�������݂��邩�`�F�b�N
if not exist "%APP_RELEASE_DIR%\\aai.exe" (
    echo �A�v���P�[�V�����������[�X�r���h���Ă�������
    pause
    exit
)

@rem release�f�B���N�g�������݂��邩�`�F�b�N
if not exist "%UPDATER_RELEASE_DIR%\\updater.exe" (
    echo �A�b�v�f�[�^�[�������[�X�r���h���Ă�������
    pause
    exit
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
copy "%UPDATER_RELEASE_DIR%\\updater.*" %OUTPUT_DIR%

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
