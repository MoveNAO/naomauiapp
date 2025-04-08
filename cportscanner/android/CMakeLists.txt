cmake_minimum_required(VERSION 3.6)

project(scanner)

add_library(scanner SHARED scanner.c)

set_target_properties(scanner PROPERTIES
    PUBLIC_HEADER "scanner.c"
)

target_include_directories(scanner PRIVATE
    ${CMAKE_SOURCE_DIR}
)