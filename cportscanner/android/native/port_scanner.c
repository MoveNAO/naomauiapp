#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <fcntl.h>
#include <errno.h>

#define PORT 5000
#define TIMEOUT_MS 200 

int scan_port_range(const char* subnet_prefix, char* output_buffer, int max_output_len) {
    int total_written = 0;

    for (int i = 1; i <= 254; i++) {
        char ip[32];
        snprintf(ip, sizeof(ip), "%s.%d", subnet_prefix, i);

        int sock = socket(AF_INET, SOCK_STREAM, 0);
        if (sock < 0) continue;

        int flags = fcntl(sock, F_GETFL, 0);
        fcntl(sock, F_SETFL, flags | O_NONBLOCK);

        struct sockaddr_in addr;
        addr.sin_family = AF_INET;
        addr.sin_port = htons(PORT);
        inet_pton(AF_INET, ip, &addr.sin_addr);

        int res = connect(sock, (struct sockaddr*)&addr, sizeof(addr));
        
        if (res < 0 && errno == EINPROGRESS) {
            fd_set fdset;
            struct timeval tv;
            
            FD_ZERO(&fdset);
            FD_SET(sock, &fdset);
            tv.tv_sec = 0;
            tv.tv_usec = TIMEOUT_MS * 1000; 
            
            if (select(sock + 1, NULL, &fdset, NULL, &tv) == 1) {
                int so_error;
                socklen_t len = sizeof(so_error);
                
                getsockopt(sock, SOL_SOCKET, SO_ERROR, &so_error, &len);
                
                if (so_error == 0) {
                    int written = snprintf(output_buffer + total_written, max_output_len - total_written, "%s\n", ip);
                    if (written <= 0 || written >= max_output_len - total_written) {
                        close(sock);
                        break;
                    }
                    total_written += written;
                }
            }
        }
        
        close(sock);
    }

    return total_written;
}