cmake_minimum_required(VERSION 3.12)

project(ODBCProg)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED 17)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED 11)

include(FindODBC)

include_directories(${ODBCProg_SOURCE_DIR}/include ${ODBC_INCLUDE_DIRS})
link_libraries(${ODBC_LIBRARIES})

add_executable(unixOdbcTest src/unixOdbcTest.c)




include(FetchContent)
FetchContent_Declare(
    googletest
    GIT_REPOSITORY https://github.com/google/googletest.git
    GIT_TAG release-1.12.1
)

FetchContent_MakeAvailable(googletest)

include(GoogleTest)