#!/usr/bin/env python3

class DbConnection:
    def __init__(self, conn, field_converter, cursor_init=None):
        self._connection = conn
        self._field_converter = field_converter
        self._cursor_init = cursor_init if cursor_init is not None else self._default_cursor_init

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        self._connection.close()

    def _default_cursor_init(self, conn):
        return conn.cursor()

    def read_first(self, cmd, converter, parameters=None, autocommit=True):
        result = None
        with self._cursor_init(self._connection) as cursor:
            if parameters:
                cursor.execute(cmd, parameters)
            else:
                cursor.execute(cmd)
            r = cursor.fetchone()
            if r:
                result = converter(r, self._field_converter, cursor.description)

        if autocommit:
            self._connection.commit()

        return result

    def read_as(self, cmd, converter, parameters=None, autocommit=True):
        result_list = []
        with self._cursor_init(self._connection) as cursor:
            if parameters:
                cursor.execute(cmd, parameters)
            else:
                cursor.execute(cmd)
            for r in cursor.fetchall():
                result = converter(r, self._field_converter, cursor.description)
                result_list.append(result)

        if autocommit:
            self._connection.commit()

        return result_list

    def execute(self, cmd, parameters=None, autocommit=True):
        affected = 0
        with self._cursor_init(self._connection) as cursor:
            if parameters:
                cursor.execute(cmd, parameters)
            else:
                cursor.execute(cmd)
            affected = cursor.rowcount

        if autocommit:
            self._connection.commit()

        return affected
