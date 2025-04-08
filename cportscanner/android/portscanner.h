#ifndef PORT_SCANNER_H
#define PORT_SCANNER_H

#ifdef __cplusplus
extern "C" {
#endif

int scan_port_range(const char* subnet_prefix, char* output_buffer, int max_output_len);

#ifdef __cplusplus
}
#endif

#endif 