#!/usr/bin/env python3

class DbConnection:
    def __init__(self, conn):
        self._connection = conn

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        self._connection.close()

    def _default_cursor_init(self):
        return self._connection.cursor()

    def read_as(self, cmd, converter, parameters=None,
                autocommit=True, cursor_creator=None):
        cursor_init = cursor_creator if cursor_creator is not None else self._default_cursor_init

        result_list = []
        with cursor_init() as cursor:
            cursor.execute(cmd, parameters)
            for r in cursor.fetchall():
                result = converter(r)
                result_list.append(result)

        if autocommit:
            self._connection.commit()

        return result_list
