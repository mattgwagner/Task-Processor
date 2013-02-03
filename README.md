Task-Processor
==============

This is some template code I wrote for handling scheduled jobs via a background .NET Windows Service.

To add jobs to the schedule, implement IJob interface from Quartz and add it to the Jobs.xml configuration file.