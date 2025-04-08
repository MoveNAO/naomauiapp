#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>

#define PORT 5000
#define TIMEOUT_SEC 1

int scan_port_range(const char* subnet_prefix, char* output_buffer, int max_output_len) {
    int total_written = 0;

    for (int i = 1; i <= 254; i++) {
        char ip[32];
        snprintf(ip, sizeof(ip), "%s.%d", subnet_prefix, i);

        int sock = socket(AF_INET, SOCK_STREAM, 0);
        if (sock < 0) continue;

        struct sockaddr_in addr;
        addr.sin_family = AF_INET;
        addr.sin_port = htons(PORT);
        inet_pton(AF_INET, ip, &addr.sin_addr);

        struct timeval timeout = { TIMEOUT_SEC, 0 };
        setsockopt(sock, SOL_SOCKET, SO_SNDTIMEO, &timeout, sizeof(timeout));

        int result = connect(sock, (struct sockaddr*)&addr, sizeof(addr));
        close(sock);

        if (result == 0) {
            int written = snprintf(output_buffer + total_written, max_output_len - total_written, "%s\n", ip);
            if (written <= 0 || written >= max_output_len - total_written) break;
            total_written += written;
        }
    }

    return total_written;
}
