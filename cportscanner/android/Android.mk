LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := portscanner
LOCAL_SRC_FILES := portscanner.c
LOCAL_LDLIBS := -llog

include $(BUILD_SHARED_LIBRARY)