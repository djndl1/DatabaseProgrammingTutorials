#!/usr/bin/env python3

import unittest

from dataclasses import dataclass

import pymysql.connections
import pyodbc
from pymysql.cursors import DictCursor

import Connection
import FieldConverter

@dataclass
class User:
    id: int
    name: str
    fullname: str

class ConnectionTest(unittest.TestCase):

    def _get_mysql_conn(self):
        conn = pymysql.connect(host="192.168.3.207", database="djn",
                               user="djn", password="freebird",
                               charset="utf8mb4", cursorclass=DictCursor)
        return Connection.DbConnection(conn, FieldConverter.DictTupleConverter)

    def _get_mssql_conn(self):
        conn = pyodbc.connect(
            'Driver={FreeTDS};Server=192.168.3.207,1433;UID=djn;PWD=Liberaavis@@djn1;Database=DJN')
        return Connection.DbConnection(conn, FieldConverter.NamedTupleConverter)

    def get_connection(self):
        #return self._get_mysql_conn()
        return self._get_mssql_conn()

    def row_to_user(self, row, field_converter):
        return User(
            field_converter(row, "ID"),
            field_converter(row, "NAME"),
            field_converter(row, "FULLNAME")
        )

    def test_simple_select(self):
        with self.get_connection() as conn:
            results = conn.read_as("""
                SELECT * FROM USER_ACCOUNT
            """, self.row_to_user)
            print(results)


    def test_parameterized_select(self):
        with self.get_connection() as conn:
            results = conn.read_first(
                "SELECT * FROM USER_ACCOUNT WHERE ID = ?",
                self.row_to_user,
                (4,)
            )
            print(results)

    def test_update(self):
        with self.get_connection() as conn:
            i = conn.execute(
                """
                UPDATE USER_ACCOUNT
                   SET
                       NAME = 'I'
                WHERE ID = ?
            """, (4,))
            print(i)
