##########################################################################
RCI: a connection interface utility
##########################################################################


.. class:: no-web

**RCI** is a file system connection utility that enables administrators and technical
stakeholders to broker and test a connection between local and remote file systems.
Currently RCI supports NFS, DFS, CIFS, Samba, NTFS and NFS.

.. class:: no-web no-pdf


.. contents::

.. section-numbering::

.. raw:: pdf

   PageBreak oneColumn

=============
Main features
=============

* Test a connection to a Network File System within the Trusted Domain (Local)
* Test a connection to a Network File Share outside of the Trusted Domain
(Remote)
* Validate permissions on the file share
* Receive verbose output on why the connection failed
* Test the speed of reading the file system objects relative to the root

=============
Running
=============

RCI is a command line utility. So the application needs to be executed through a
supported Windows Shell. (Powershell or Cmd)

2.1 As a first step place the RCI.exe file in a desired location on the test machine.

2.2 Open the desired shell with Administrators privileges by right clicking on the application icon and selecting “Run As Administrator”

2.3 Navigate to the path where the executable was dropped by typing the following command:

.. code-block:: bash

$ cd path_to_executable

The command line application should then show the current directory as the location of
the RCI executable file

2.4 Execute the command to test the File Share connection and the import (optional):

.. code-block:: bash

$ RCI [-r or –l] [Path to Share] –u [Full Username]

Review the Parameter Information and Output Definition sections for detailed
information on what each parameter represents and what options are available

2.4 RCI will ask for the password of the user’s credentials to use for connecting to 
the share. Enter the password and press [Enter]

2.5 RCI will connect to the local or remote share and provide verbose output of the
result. In the case of a remote share RCI will return with either a descriptive error
or with an error code in case where the error is unknown.

=============
Parameter Information
=============

The following table provides information and definition of the various command line
arguments for the RCI utility:

Parameter Description

-l Indicates that the RCI utility should attempt a connection inside the trusted
domain by using Windows Impersonation for security and accessibility

-r Instructs the RCI utility to connect to a file share outside of the trusted
domain and typically with a UNC path (\\share).

Path The path to connect to the File Share (e.g. C:\ or \\Share)

-u Indicates that the information after this parameter is the username of the
user to connect to the share with
Username The full username of the user. For example UserDoman\username

-i This optional (silent) parameter instructs RCI to also mimic a file share
import

=============
Output Definition
=============

The following section defines what each output line means and what to do when RCI
provides this output:

After running the utility in the command line as per step 2.4 a connection attempt will be
made with the provided details:

.. code-block:: bash

$ Connecting to [Local/Remote] Share...

If the connection succeeded RCI will provide output of this:

.. code-block:: bash

$ Successfully connected to the share at: [Path]

If the connection failed RCI will provide output of this. See the following screenshots as
two separate examples:

Unknown error occurred. Please provide this error information when it occurs so we can map it to
the system error codes

Authentication failed for the username and password provided
Once the connection has succeeded RCI will attempt to verify that there are Read and
Write permissions on the share. The following is example output:

.. code-block:: bash

$ Sufficient Permissions Exist For User [username]

OPTIONAL OUTPUT:

If specifying the “-i” parameter when running step 2.4 RCI will attempt to mimic the
import operation and will read all the file system objects.
This is useful in many ways. The output indicates when it started, when it finished and
how many objects was discovered.
Below is an example of this output:


----------
Change log
----------

See `CHANGELOG <https://github.com/kryptogeek/RCI/blob/master/CHANGELOG.rst>`_.

-------
Authors
-------

`Isak Bosman`_  (`@kryptogeek`_) created RCI`_