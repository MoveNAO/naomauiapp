LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := portscanner
LOCAL_SRC_FILES := port_scanner.c
LOCAL_LDLIBS := -llog

include $(BUILD_SHARED_LIBRARY)