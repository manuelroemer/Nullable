# Changelog

# v1.1.0

This release updates the package, so that the attributes file is no longer included in projects
targeting a framework >= .NET Standard 2.1.
For any target version below, the file is still automatically included.
This ensures that the package can easily be installed in libraries which are multi-targeting
different frameworks, without them having to define conditional compilation constants.

In addition, a small typo in the XML documentation was fixed.


# v1.0.0

The initial release.

This release covers the following attributes:

* `AllowNullAttribute`
* `DisallowNullAttribute`
* `DoesNotReturnAttribute`
* `DoesNotReturnIfAttribute`
* `MaybeNullAttribute`
* `MaybeNullWhenAttribute`
* `NotNullAttribute`
* `NotNullIfNotNullAttribute`
* `NotNullWhenAttribute`

Furthermore, the following compiler directives are supported in the source file:

* `NULLABLE_ATTRIBUTES_DISABLE`
* `NULLABLE_ATTRIBUTES_EXCLUDE_FROM_CODE_COVERAGE`