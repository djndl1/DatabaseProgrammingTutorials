#!/usr/bin/env python3

import unittest

from dataclasses import dataclass

import pymysql.connections
from pymysql.cursors import DictCursor

import Connection

@dataclass
class User:
    id: int
    name: str
    fullname: str

class ConnectionTest(unittest.TestCase):
    def get_connection(self):
        conn = pymysql.connect(host="192.168.3.207", database="djn",
                               user="djn", password="freebird",
                               charset="utf8mb4", cursorclass=DictCursor)
        return Connection.DbConnection(conn)

    def row_to_user(self, row):
        return User(
            row["ID"],
            row["NAME"],
            row["FULLNAME"]
        )

    def test_simple_select(self):
        with self.get_connection() as conn:
            results = conn.read_as("SELECT * FROM USER_ACCOUNT", self.row_to_user)
            print(results)


    def test_parameterized_select(self):
        with self.get_connection() as conn:
            results = conn.read_as("SELECT * FROM USER_ACCOUNT WHERE ID = %(id)s",
                                   self.row_to_user, {
                                       "id": 3
                                   })
            print(results)
