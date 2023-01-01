#include <sql.h>
#include <sqlext.h>
#include <sqltypes.h>
#include <stdio.h>
#include <stdlib.h>


char V_OD_stat[10]; // Status SQL
char V_OD_msg[200], V_OD_buffer[200];

SQLINTEGER V_OD_err, V_OD_rowanz, V_OD_id;
SQLSMALLINT V_OD_mlen;

int main(int argc, char *argv[])
{
    long V_OD_erg; // result of functions

	// 1. allocate Environment handle and register version
	SQLHENV V_OD_Env; // Handle ODBC environment
	V_OD_erg = SQLAllocHandle(SQL_HANDLE_ENV, SQL_NULL_HANDLE, &V_OD_Env);
	if ((V_OD_erg != SQL_SUCCESS) && (V_OD_erg != SQL_SUCCESS_WITH_INFO)) {
		printf("Error AllocHandle\n");
		exit(0);
	}

	V_OD_erg = SQLSetEnvAttr(V_OD_Env, SQL_ATTR_ODBC_VERSION, (void *)SQL_OV_ODBC3, 0);
	if ((V_OD_erg != SQL_SUCCESS) && (V_OD_erg != SQL_SUCCESS_WITH_INFO)) {
		printf("Error SetEnv\n");
		SQLFreeHandle(SQL_HANDLE_ENV, V_OD_Env);
		exit(0);
	}
	// 2. allocate connection handle, set timeout
    SQLHDBC V_OD_hdbc; // Handle connection
	V_OD_erg = SQLAllocHandle(SQL_HANDLE_DBC, V_OD_Env, &V_OD_hdbc);
	if ((V_OD_erg != SQL_SUCCESS) && (V_OD_erg != SQL_SUCCESS_WITH_INFO)) {
		printf("Error AllocHDB %ld\n", V_OD_erg);
		SQLFreeHandle(SQL_HANDLE_ENV, V_OD_Env);
		exit(0);
	}

	SQLSetConnectAttr(V_OD_hdbc, SQL_LOGIN_TIMEOUT, (SQLPOINTER *)5, 0);
	// 3. Connect to the datasource "web"
	V_OD_erg = SQLConnect(V_OD_hdbc, (SQLCHAR *)"web", SQL_NTS, (SQLCHAR *)"sqlalchemy", SQL_NTS, (SQLCHAR *)"sqlalchemy", SQL_NTS);
	if ((V_OD_erg != SQL_SUCCESS) && (V_OD_erg != SQL_SUCCESS_WITH_INFO)) {
		printf("Error SQLConnect %ld\n", V_OD_erg);
		SQLGetDiagRec(SQL_HANDLE_DBC, V_OD_hdbc, 1, V_OD_stat, &V_OD_err, V_OD_msg, 100, &V_OD_mlen);
		printf("%s (%d)\n", V_OD_msg, V_OD_err);
		SQLFreeHandle(SQL_HANDLE_ENV, V_OD_Env);
		exit(0);
	}
	printf("Connected !\n");
	/* continued on next page */
}