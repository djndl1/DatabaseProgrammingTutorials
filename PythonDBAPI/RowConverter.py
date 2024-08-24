#!/usr/bin/env python3

def DictRowConverter(row_converter):
    def wrapped(row, field_converter, description):
        r = row
        if isinstance(r, tuple) or isinstance(r, list):
            columns = [col[0] for col in description]
            r = dict(zip(columns, r))

        return row_converter(r, field_converter, description)

    return wrapped
