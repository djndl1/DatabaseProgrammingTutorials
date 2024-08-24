#!/usr/bin/env python3


def NamedTupleConverter(row, field):
    return getattr(row, field)

def DictConverter(row, field):
    return row[field]

def TupleConverter(row, index):
    return row[index]
