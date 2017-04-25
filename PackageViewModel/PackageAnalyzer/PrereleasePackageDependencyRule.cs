﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Globalization;
using NuGetPe;
using NuGetPackageExplorer.Types;

namespace PackageExplorerViewModel.Rules
{
    [Export(typeof(IPackageRule))]
    internal class PrereleasePackageDependencyRule : IPackageRule
    {
        #region IPackageRule Members

        public IEnumerable<PackageIssue> Validate(IPackage package, string packagePath)
        {
            if (IsPreReleasedVersion(package.Version))
            {
                return new PackageIssue[0];
            }

            return package.DependencySets.SelectMany(p => p.Dependencies)
                                         .Where(IsPrereleaseDependency)
                                         .Select(CreatePackageIssue);
        }

        #endregion

        private static bool IsPrereleaseDependency(PackageDependency pd)
        {
            if (pd.VersionSpec == null)
            {
                return false;
            }

            return IsPreReleasedVersion(pd.VersionSpec.MinVersion) || IsPreReleasedVersion(pd.VersionSpec.MaxVersion);
        }

        private static bool IsPreReleasedVersion(NuGet.SemanticVersion version)
        {
            return version != null && !String.IsNullOrEmpty(version.SpecialVersion);
        }

        private static bool IsPreReleasedVersion(TemplatebleSemanticVersion version)
        {
            return version != null && !String.IsNullOrEmpty(version.SpecialVersion);
        }


        private static PackageIssue CreatePackageIssue(PackageDependency target)
        {
            return new PackageIssue(
                PackageIssueLevel.Error,
                "Invalid prerelease dependency",
                String.Format(CultureInfo.CurrentCulture,
                              "A stable release of a package must not have a dependency on a prerelease package, '{0}'.",
                              target),
                String.Format(CultureInfo.CurrentCulture,
                              "Either modify the version spec of dependency '{0}' or update the version field.", target)
                );
        }
    }
}