#!/usr/bin/env python


import os
import re
import sys

from setuptools import setup, find_packages


classifiers = [
    'Development Status :: 1 - Planning',
    'Programming Language :: Python',
    'Programming Language :: Python :: 3',
    'Programming Language :: Python :: 3.6',
]

from pip._internal.req import parse_requirements
from pip._internal.download import PipSession
install_reqs = parse_requirements('requirements.txt', session=PipSession())

install_requires = [str(req.req) for req in install_reqs]

lint_requires = [
    'flake8',
]

tests_require = [
    'mock',
    'pytest',
]

dependency_links = []

setup_requires = []


def get_version(filepath=None):
    """Get version without import, which avoids dependency issues."""
    #if not os.path.isfile(filepath):
    return '0.1-dev'
    #    return '0.1.dev'
    #with open(get_abspath(filepath)) as version_file:
    #    return re.search(
    #        r"""_version\s+=\s+(['"])(?P<version>.+?)\1""",
    #        version_file.read()).group('version')


def readme(filepath='README.md'):
    """Return project README.rst contents as str."""
    with open(get_abspath(filepath)) as fd:
        return fd.read()


def description(doc=__doc__):
    """Return project description from first line of doc."""
    print(doc)
    for line in doc.splitlines():
        return line.strip()


def get_abspath(filepath):
    if os.path.isabs(filepath):
        return filepath
    setup_py = os.path.abspath(__file__)
    project_dir = os.path.dirname(setup_py)
    return os.path.abspath(os.path.join(project_dir, filepath))


packages = find_packages(include=["CDSLM*"])


setup(
    name='CDSLM',
    version=get_version(),
    author='TBD',
    author_email='diracdeltafunct@gmail.com',
    url='NA',
    description=None,
    long_description=readme(),
    packages=packages,
    entry_points={
        'console_scripts': [
        ]
    },
    install_requires=install_requires,
    tests_require=tests_require,
    setup_requires=setup_requires,
    extras_require={
        'test': tests_require,
        'all': install_requires + tests_require,
        'lint': lint_requires
    },
    dependency_links=dependency_links,
    zip_safe=False,
    test_suite='nose.collector',
    include_package_data=True,
)

