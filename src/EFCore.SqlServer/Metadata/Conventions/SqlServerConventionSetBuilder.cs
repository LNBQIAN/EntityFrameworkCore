﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerConventionSetBuilder : RelationalConventionSetBuilder
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerConventionSetBuilder(
            [NotNull] RelationalConventionSetBuilderDependencies dependencies,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(dependencies)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            var valueGenerationStrategyConvention = new SqlServerValueGenerationStrategyConvention();
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
            conventionSet.ModelInitializedConventions.Add(new RelationalMaxIdentifierLengthConvention(128));

            ValueGeneratorConvention valueGeneratorConvention = new SqlServerValueGeneratorConvention();
            ReplaceConvention(conventionSet.BaseEntityTypeChangedConventions, valueGeneratorConvention);

            var sqlServerInMemoryTablesConvention = new SqlServerMemoryOptimizedTablesConvention();
            conventionSet.EntityTypeAnnotationChangedConventions.Add(sqlServerInMemoryTablesConvention);

            ReplaceConvention(conventionSet.PrimaryKeyChangedConventions, valueGeneratorConvention);

            conventionSet.KeyAddedConventions.Add(sqlServerInMemoryTablesConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGeneratorConvention);

            var sqlServerIndexConvention = new SqlServerIndexConvention(_sqlGenerationHelper);
            conventionSet.IndexAddedConventions.Add(sqlServerInMemoryTablesConvention);
            conventionSet.IndexAddedConventions.Add(sqlServerIndexConvention);

            conventionSet.IndexUniquenessChangedConventions.Add(sqlServerIndexConvention);

            conventionSet.IndexAnnotationChangedConventions.Add(sqlServerIndexConvention);

            conventionSet.PropertyNullabilityChangedConventions.Add(sqlServerIndexConvention);

            conventionSet.PropertyAnnotationChangedConventions.Add(sqlServerIndexConvention);
            conventionSet.PropertyAnnotationChangedConventions.Add((SqlServerValueGeneratorConvention)valueGeneratorConvention);

            ReplaceConvention(conventionSet.ModelAnnotationChangedConventions, (RelationalDbFunctionConvention)new SqlServerDbFunctionConvention());

            return conventionSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ConventionSet Build()
        {
            var coreTypeMapperDependencies = new CoreTypeMapperDependencies(
                new ValueConverterSelector(
                    new ValueConverterSelectorDependencies()));

            var sqlServerTypeMapper = new SqlServerTypeMapper(
                new RelationalTypeMapperDependencies());

            var convertingTypeMapper = new FallbackRelationalCoreTypeMapper(
                coreTypeMapperDependencies,
                new RelationalTypeMapperDependencies(), 
                sqlServerTypeMapper);

            return new SqlServerConventionSetBuilder(
                new RelationalConventionSetBuilderDependencies(convertingTypeMapper, null, null, null),
                new SqlServerSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()))
                .AddConventions(
                    new CoreConventionSetBuilder(
                        new CoreConventionSetBuilderDependencies(convertingTypeMapper, null, null, null))
                        .CreateConventionSet());
        }
    }
}
