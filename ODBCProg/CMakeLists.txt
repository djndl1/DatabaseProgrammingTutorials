cmake_minimum_required(VERSION 3.12)

project(ODBCProg)

include(FindODBC)

include_directories(${ODBCProg_SOURCE_DIR}/include ${ODBC_INCLUDE_DIRS})
link_libraries(${ODBC_LIBRARIES})

add_executable(unixOdbcTest src/unixOdbcTest.c)