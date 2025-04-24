@echo off

:: Imposta il percorso dell'NDK
set NDK_PATH=D:\androidsdk\ndk\29.0.13113456

:: Imposta il percorso del tuo progetto
set NDK_PROJECT_PATH=%cd%\native

:: Esegui ndk-build
"%NDK_PATH%\ndk-build.cmd" -C "%NDK_PROJECT_PATH%"
