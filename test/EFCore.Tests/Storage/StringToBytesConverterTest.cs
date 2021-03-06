﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class StringToBytesConverterTest
    {
        private static readonly StringToBytesConverter _stringToUtf8Converter
            = new StringToBytesConverter(Encoding.UTF8);

        [Fact]
        public void Can_convert_strings_to_UTF8()
        {
            var converter = _stringToUtf8Converter.ConvertToStoreExpression.Compile();

            Assert.Equal(new byte[] { 83, 112, 196, 177, 110, 204, 136, 97, 108, 32, 84, 97, 112 }, converter("Spın̈al Tap"));
            Assert.Equal(new byte[0], converter(""));
            Assert.Null(converter(null));
        }

        [Fact]
        public void Can_convert_UTF8_to_strings()
        {
            var converter = _stringToUtf8Converter.ConvertFromStoreExpression.Compile();

            Assert.Equal("Spın̈al Tap", converter(new byte[] { 83, 112, 196, 177, 110, 204, 136, 97, 108, 32, 84, 97, 112 }));
            Assert.Equal("", converter(new byte[0]));
            Assert.Null(converter(null));
        }
    }
}
