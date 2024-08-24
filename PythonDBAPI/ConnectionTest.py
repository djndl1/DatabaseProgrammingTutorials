#!/usr/bin/env python3

import unittest

from dataclasses import dataclass

import pymysql.connections
import pyodbc
import oracledb
from pymysql.cursors import DictCursor

import Connection
import FieldConverter
from RowConverter import DictRowConverter

@dataclass
class User:
    id: int
    name: str
    fullname: str


@DictRowConverter
def row_to_user(row, field_converter, description):
    return User(
        field_converter(row, "ID"),
        field_converter(row, "NAME"),
        field_converter(row, "FULLNAME")
    )


class ConnectionTest(unittest.TestCase):

    def _get_mysql_conn(self):
        conn = pymysql.connect(host="192.168.3.207", database="djn",
                               user="djn", password="freebird",
                               charset="utf8mb4", cursorclass=DictCursor)
        return Connection.DbConnection(conn, FieldConverter.DictConverter)

    def _get_mssql_conn(self):
        conn = pyodbc.connect(
            'Driver={FreeTDS};Server=192.168.3.207,1433;UID=djn;PWD=Liberaavis@@djn1;Database=DJN')
        return Connection.DbConnection(conn, FieldConverter.NamedTupleConverter)

    def _get_oracle_conn(self):
        conn = oracledb.connect(user='DJN1',
                                password='freebird',
                                dsn='192.168.3.207:1521/XE')
        return Connection.DbConnection(conn,
                                       FieldConverter.DictConverter)

    def _get_connection(self):
        #return self._get_mysql_conn()
        #return self._get_mssql_conn()
        return self._get_oracle_conn()

    def test_simple_select(self):
        with self._get_connection() as conn:
            results = conn.read_as("""
                SELECT * FROM USER_ACCOUNT
            """, row_to_user)
            print(results)
            oracledb.Cursor


    def test_parameterized_select(self):
        with self._get_connection() as conn:
            results = conn.read_first(
                "SELECT * FROM USER_ACCOUNT WHERE ID = :id",
                row_to_user,
                { "id": 4 }
            )
            print(results)

    def test_update(self):
        with self._get_connection() as conn:
            i = conn.execute(
                """
                UPDATE USER_ACCOUNT
                   SET
                       NAME = 'Oracle'
                WHERE ID = :id
            """,
                { "id": 4 })
            print(i)
