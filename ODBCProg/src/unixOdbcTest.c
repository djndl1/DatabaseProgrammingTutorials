#include <sql.h>
#include <sqlext.h>
#include <sqltypes.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>

typedef struct _type_column {
	char *column_name;
	int column_number;
	SQLSMALLINT column_type_c_identifier;
} type_column_t;

type_column_t type_columns[] = {
	{ "TYPE_NAME", 1, SQL_C_CHAR },		  { "DATA_TYPE", 2, SQL_C_SHORT },		{ "COLUMN_SIZE", 3, SQL_C_LONG },
	{ "MINIMUM_SCALE", 14, SQL_C_SHORT }, { "MAXIMUM_SCALE", 15, SQL_C_SHORT }, { "SQL_DATA_TYPE", 16, SQL_C_SHORT },
};

typedef struct _odbc_type_info {
	char type_name[100];
	SQLSMALLINT data_type;
	SQLINTEGER column_size;
	SQLSMALLINT minimum_scale;
	SQLSMALLINT maximum_scale;
	SQLSMALLINT sql_data_type;
} odbc_type_info_t;

static char *get_sql_type_name(SQLSMALLINT type_id)
{
	switch (type_id) {
	case SQL_CHAR:
		return "SQL_CHAR";
	case SQL_VARCHAR:
		return "SQL_VARCHAR";
	case SQL_LONGVARCHAR:
		return "SQL_LONGVARCHAR";
	case SQL_DECIMAL:
		return "SQL_DECIMAL";
	case SQL_NUMERIC:
		return "SQL_NUMERIC";
	case SQL_SMALLINT:
		return "SQL_SMALLINT";
	case SQL_INTEGER:
		return "SQL_INTEGER";
	case SQL_REAL:
		return "SQL_REAL";
	case SQL_FLOAT:
		return "SQL_FLOAT";
	case SQL_DOUBLE:
		return "SQL_DOUBLE";
	case SQL_BIT:
		return "SQL_BIT";
	case SQL_TINYINT:
		return "SQL_TINYINT";
	case SQL_BIGINT:
		return "SQL_BIGINT";
	case SQL_BINARY:
		return "SQL_BINARY";
	case SQL_VARBINARY:
		return "SQL_VARBINARY";
	case SQL_LONGVARBINARY:
		return "SQL_LONGVARBINARY";
	case SQL_TYPE_DATE:
		return "SQL_TYPE_DATE";
	case SQL_TYPE_TIME:
		return "SQL_TYPE_TIME";
	case SQL_TYPE_TIMESTAMP:
		return "SQL_TYPE_TIMESTAMP";
	case SQL_GUID:
		return "SQL_GUID";
	default:
		return "Unknown SQL type";
	}
}

static SQLRETURN display_oracle_types(SQLHDBC connection)
{
	SQLRETURN ret = SQL_SUCCESS;

	SQLHSTMT select_stmt;
	SQLRETURN alloc_result = SQLAllocHandle(SQL_HANDLE_STMT, connection, &select_stmt);
	if (!SQL_SUCCEEDED(alloc_result)) {
		return alloc_result;
	}

	SQLGetTypeInfo(select_stmt, SQL_ALL_TYPES);

	odbc_type_info_t result_buffer = { 0 };
	SQLBindCol(select_stmt, 1, SQL_C_CHAR, &result_buffer.type_name, sizeof(result_buffer.type_name), NULL);
	SQLBindCol(select_stmt, 2, SQL_C_SHORT, &result_buffer.data_type, sizeof(result_buffer.data_type), NULL);
	SQLBindCol(select_stmt, 3, SQL_C_LONG, &result_buffer.column_size, sizeof(result_buffer.column_size), NULL);
	SQLBindCol(select_stmt, 14, SQL_C_SHORT, &result_buffer.minimum_scale, sizeof(result_buffer.minimum_scale), NULL);
	SQLBindCol(select_stmt, 15, SQL_C_SHORT, &result_buffer.maximum_scale, sizeof(result_buffer.maximum_scale), NULL);
	SQLBindCol(select_stmt, 16, SQL_C_SHORT, &result_buffer.sql_data_type, sizeof(result_buffer.sql_data_type), NULL);

	for (int i = 0; i < sizeof(type_columns) / sizeof(type_columns[0]); i++) {
		printf("%s\t\t", type_columns[i].column_name);
	}

	fputc('\n', stdout);
	SQLRETURN fetch_ret;
	while ((fetch_ret = SQLFetch(select_stmt)) != SQL_NO_DATA) {
		if (!(SQL_SUCCEEDED(fetch_ret))) {
			ret = fetch_ret;

			SQLCHAR err_msg[200], status[6];
			SQLSMALLINT msg_len = 0;
			SQLINTEGER native_error;
			SQLGetDiagRec(SQL_HANDLE_STMT, select_stmt, 1, status, &native_error, err_msg, 200, &msg_len);

			fprintf(stderr, "Error %s\n", err_msg);

			goto FreeStatement;
		}

		char *type_name = get_sql_type_name(result_buffer.data_type);

		printf("%s\t\t%s\t\t%d\t\t%d\t\t%d\t\t%d\n", result_buffer.type_name, type_name, result_buffer.column_size,
			   result_buffer.minimum_scale, result_buffer.maximum_scale, result_buffer.sql_data_type);
	}

FreeStatement:
	SQLFreeHandle(SQL_HANDLE_STMT, select_stmt);
	return ret;
}

static SQLRETURN insert_user(SQLHDBC connection)
{
	SQLRETURN ret = SQL_SUCCESS;
	const char *insert_sql = "INSERT INTO \"tkeyuser\" (\"dtname\") VALUES (?)";
	const char *new_user = "NEW_USER";

	SQLHSTMT insert_stmt;
	SQLRETURN alloc_result = SQLAllocHandle(SQL_HANDLE_STMT, connection, &insert_stmt);
	if (!SQL_SUCCEEDED(alloc_result)) {
		return alloc_result;
	}
    SQLRETURN prepare_ret = SQLPrepare(insert_stmt, (SQLCHAR*)insert_sql, (SQLINTEGER)SQL_NTS);
	if (!SQL_SUCCEEDED(prepare_ret)) {
		ret = prepare_ret;
        goto FreeStatement;
	}
	SQLBindParameter(insert_stmt, 1, SQL_PARAM_INPUT, SQL_C_CHAR, SQL_VARCHAR, 40, 0, (SQLPOINTER)new_user, strlen(new_user) + 1,
					 NULL);

    SQLRETURN exec_ret = SQLExecute(insert_stmt);
	if (!SQL_SUCCEEDED(exec_ret)) {
		ret = exec_ret;
        goto FreeStatement;
	}

FreeStatement:
	SQLFreeHandle(SQL_HANDLE_STMT, insert_stmt);
	return ret;
}

static SQLRETURN print_tkeyuser(SQLHDBC connection)
{
	SQLRETURN ret = SQL_SUCCESS;
	const char *query_sql = "SELECT \"iduser\",\"dtname\" FROM \"tkeyuser\" ORDER BY \"iduser\"";

	SQLUINTEGER id_column = -1;
	char name_column[45];

	SQLHSTMT select_stmt;
	SQLRETURN alloc_result = SQLAllocHandle(SQL_HANDLE_STMT, connection, &select_stmt);
	if (!SQL_SUCCEEDED(alloc_result)) {
		return alloc_result;
	}

	SQLLEN id_col_len;
	SQLRETURN id_bind_ret = SQLBindCol(select_stmt, 1, SQL_C_ULONG, &id_column, sizeof(id_column), &id_col_len);
	if (!SQL_SUCCEEDED(id_bind_ret)) {
		ret = id_bind_ret;
		goto FreeStatement;
	}

	SQLLEN name_col_len;
	SQLRETURN name_bind_ret = SQLBindCol(select_stmt, 2, SQL_C_CHAR, name_column, sizeof(name_column), &name_col_len);
	if (!SQL_SUCCEEDED(name_bind_ret)) {
		ret = name_bind_ret;
		goto FreeStatement;
	}

	SQLRETURN exec_ret = SQLExecDirect(select_stmt, (SQLCHAR *)query_sql, SQL_NTS);
	if (!(SQL_SUCCEEDED(exec_ret))) {
		ret = exec_ret;

		SQLCHAR err_msg[200], status[6];
		SQLSMALLINT msg_len = 0;
		SQLINTEGER native_error;
		SQLGetDiagRec(SQL_HANDLE_STMT, select_stmt, 1, status, &native_error, err_msg, 200, &msg_len);

		fprintf(stderr, "Error %s\n", err_msg);
		goto FreeStatement;
	}

	SQLLEN row_cnt = 0;
	SQLRowCount(select_stmt, &row_cnt);

	SQLRETURN fetch_ret;
	while ((fetch_ret = SQLFetch(select_stmt)) != SQL_NO_DATA) {
		if (!(SQL_SUCCEEDED(fetch_ret))) {
			ret = fetch_ret;

			SQLCHAR err_msg[200], status[6];
			SQLSMALLINT msg_len = 0;
			SQLINTEGER native_error;
			SQLGetDiagRec(SQL_HANDLE_STMT, select_stmt, 1, status, &native_error, err_msg, 200, &msg_len);

			fprintf(stderr, "Error %s\n", err_msg);

			goto FreeStatement;
		}

		printf("%u\t%s\n", id_column, name_column);
	}

FreeStatement:
	SQLFreeHandle(SQL_HANDLE_STMT, select_stmt);
	return ret;
}

int main(int argc, char *argv[])
{
	long result; // result of functions

	// 1. allocate Environment handle and register version
	SQLHENV env_handle; // Handle ODBC environment
	result = SQLAllocHandle(SQL_HANDLE_ENV, SQL_NULL_HANDLE, &env_handle);
	if ((result != SQL_SUCCESS) && (result != SQL_SUCCESS_WITH_INFO)) {
		fprintf(stderr, "Error AllocHandle\n");
		return -1;
	}

	result = SQLSetEnvAttr(env_handle, SQL_ATTR_ODBC_VERSION, (SQLPOINTER)SQL_OV_ODBC3, 0);
	if ((result != SQL_SUCCESS) && (result != SQL_SUCCESS_WITH_INFO)) {
		fprintf(stderr, "Error SetEnv\n");
		goto FreeEnvironmentHandle;
	}
	// 2. allocate connection handle, set timeout
	SQLHDBC connection_handle; // Handle connection
	result = SQLAllocHandle(SQL_HANDLE_DBC, env_handle, &connection_handle);
	if ((result != SQL_SUCCESS) && (result != SQL_SUCCESS_WITH_INFO)) {
		fprintf(stderr, "Error AllocHDB %ld\n", result);
		goto FreeEnvironmentHandle;
	}

	SQLSetConnectAttr(connection_handle, SQL_LOGIN_TIMEOUT, (SQLPOINTER)5, 0);
	// 3. Connect to the datasource "web"
	result = SQLConnect(connection_handle, (SQLCHAR *)"sqlalchemy_test", SQL_NTS, (SQLCHAR *)"sqlalchemy", SQL_NTS,
						(SQLCHAR *)"sqlalchemy", SQL_NTS);
	if ((result != SQL_SUCCESS) && (result != SQL_SUCCESS_WITH_INFO)) {
		fprintf(stderr, "Error SQLConnect %ld\n", result);
		goto FreeEnvironmentHandle;
	}

	printf("Connected !\n");

	result = print_tkeyuser(connection_handle);
	if (!SQL_SUCCEEDED(result)) {
		fprintf(stderr, "Error print_tkeyuser %ld\n", result);
		goto DropConnection;
	}
    insert_user(connection_handle);
	result = print_tkeyuser(connection_handle);

	display_oracle_types(connection_handle);

DropConnection:
	SQLDisconnect(connection_handle);
	SQLFreeHandle(SQL_HANDLE_DBC, connection_handle);

FreeEnvironmentHandle:
	SQLFreeHandle(SQL_HANDLE_ENV, env_handle);
	return 0;
}