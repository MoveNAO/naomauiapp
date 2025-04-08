#define _WINSOCK_DEPRECATED_NO_WARNINGS
#include <stdio.h>
#include <winsock2.h>
#include <windows.h>
#include <process.h>

#pragma comment(lib, "ws2_32.lib")

#define PORT 5000
#define MAX_THREADS 64
#define MAX_IPS 254

typedef struct {
    char ip[32];
    int result;
} ScanResult;

typedef struct {
    char subnet[20];
    int start_index;
    int end_index;
    ScanResult* results;
} ThreadData;

unsigned __stdcall scan_thread(void* arg) {
    ThreadData* data = (ThreadData*)arg;
    WSADATA wsa;
    WSAStartup(MAKEWORD(2, 2), &wsa);

    for (int i = data->start_index; i <= data->end_index; i++) {
        SOCKET sock = socket(AF_INET, SOCK_STREAM, 0);
        if (sock == INVALID_SOCKET)
            continue;

        u_long mode = 1;
        ioctlsocket(sock, FIONBIO, &mode);

        // Prepare address
        struct sockaddr_in server;
        sprintf(data->results[i-1].ip, "%s.%d", data->subnet, i);
        server.sin_addr.s_addr = inet_addr(data->results[i-1].ip);
        server.sin_family = AF_INET;
        server.sin_port = htons(PORT);

        connect(sock, (struct sockaddr*)&server, sizeof(server));

        fd_set writefds;
        struct timeval timeout;
        FD_ZERO(&writefds);
        FD_SET(sock, &writefds);
        timeout.tv_sec = 0;
        timeout.tv_usec = 100000;  // questo è timeout

        int result = select(0, NULL, &writefds, NULL, &timeout);
        data->results[i-1].result = result > 0;

        closesocket(sock);
    }

    WSACleanup();
    _endthreadex(0);
    return 0;
}

int main(int argc, char* argv[]) {
    if (argc != 2) {
        printf("Usage: %s <subnet>\n", argv[0]);
        return 1;
    }

    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) {
        printf("WSA Initialization failed: %d\n", WSAGetLastError());
        return 1;
    }
    

    ScanResult results[MAX_IPS];
    memset(results, 0, sizeof(results));
    
    HANDLE threads[MAX_THREADS];
    ThreadData thread_data[MAX_THREADS];
    
    int ips_per_thread = MAX_IPS / MAX_THREADS;
    if (MAX_IPS % MAX_THREADS != 0) ips_per_thread++;
    
    for (int i = 0; i < MAX_THREADS; i++) {
        int start_index = i * ips_per_thread + 1;
        int end_index = (i + 1) * ips_per_thread;
        
        if (end_index > MAX_IPS)
            end_index = MAX_IPS;
        
        if (start_index > MAX_IPS)
            break;
        
        strcpy(thread_data[i].subnet, argv[1]);
        thread_data[i].start_index = start_index;
        thread_data[i].end_index = end_index;
        thread_data[i].results = results;
        
        threads[i] = (HANDLE)_beginthreadex(NULL, 0, scan_thread, &thread_data[i], 0, NULL);
    }
    
    WaitForMultipleObjects(MAX_THREADS, threads, TRUE, INFINITE);
    
    for (int i = 0; i < MAX_THREADS; i++) {
        CloseHandle(threads[i]);
    }
    
    for (int i = 0; i < MAX_IPS; i++) {
        if (results[i].result) {
            printf("%s\n", results[i].ip);
        }
    }
    
    WSACleanup();
    return 0;
}