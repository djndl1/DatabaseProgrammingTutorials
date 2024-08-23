#!/usr/bin/env python3

class DbConnection:
    def __init__(self, conn, field_converter):
        self._connection = conn
        self._field_converter = field_converter

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        self._connection.close()

    def _default_cursor_init(self):
        return self._connection.cursor()

    def read_first(self, cmd, converter, parameters=None,
                   autocommit=True, cursor_creator=None):
        cursor_init = cursor_creator if cursor_creator is not None else self._default_cursor_init

        result = None
        with cursor_init() as cursor:
            if parameters:
                cursor.execute(cmd, parameters)
            else:
                cursor.execute(cmd)
            r = cursor.fetchone()
            if r:
                result = converter(r, self._field_converter)

        if autocommit:
            self._connection.commit()

        return result

    def read_as(self, cmd, converter, parameters=None,
                autocommit=True, cursor_creator=None):
        cursor_init = cursor_creator if cursor_creator is not None else self._default_cursor_init

        result_list = []
        with cursor_init() as cursor:
            if parameters:
                cursor.execute(cmd, parameters)
            else:
                cursor.execute(cmd)
            for r in cursor.fetchall():
                result = converter(r, self._field_converter)
                result_list.append(result)

        if autocommit:
            self._connection.commit()

        return result_list

    def execute(self, cmd, parameters=None,
                autocommit=True, cursor_creator=None):
        cursor_init = cursor_creator if cursor_creator is not None else self._default_cursor_init

        affected = 0
        with cursor_init() as cursor:
            if parameters:
                cursor.execute(cmd, parameters)
            else:
                cursor.execute(cmd)
            affected = cursor.rowcount

        if autocommit:
            self._connection.commit()

        return affected
