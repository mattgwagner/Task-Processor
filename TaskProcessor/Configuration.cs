using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TaskProcessor
{
    /// <summary>
    /// Represents the configuration file containing the configured jobs and their schedule
    /// </summary>
    public class Configuration
    {
        public List<JobConfig> Jobs = new List<JobConfig>();

        public class JobConfig
        {
            /// <summary>
            /// The unique name of the job, as stored in the scheduler
            /// </summary>
            public String Name { get; set; }

            /// <summary>
            /// The fully qualified (with namespace) job class name
            /// </summary>
            public String Class { get; set; }

            /// <summary>
            /// Whether or not the job is enabled for running
            /// </summary>
            public Boolean Enabled { get; set; }

            /// <summary>
            /// The schedule for the job to run (seconds minutes hours day-of-month month day-of-week)
            /// </summary>
            /// <remarks>
            /// See http://quartznet.sourceforge.net/tutorial/lesson_6.html for cron trigger explanation
            /// </remarks>
            public String CronTrigger { get; set; }

            /// <summary>
            /// Comments about the particular job, i.e. what it's doing or why
            /// </summary>
            public String Comments { get; set; }

            /// <summary>
            /// Generates the Quartz specific job object, populated with the current information
            /// </summary>
            public JobDetailImpl GenerateJob()
            {
                var job = new JobDetailImpl(this.Name, Type.GetType(this.Class))
                {
                    Description = this.Comments
                };

                return job;
            }
        }

        /// <summary>
        /// Load the configuration from an XML file at the given path
        /// </summary>
        public static Configuration LoadFromFile(String filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                return new XmlSerializer(typeof(Configuration)).Deserialize(stream) as Configuration;
            }
        }

        /// <summary>
        /// Saves the given configuration to the provided file path
        /// </summary>
        public static void SaveToFile(Configuration configuration, String filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                new XmlSerializer(typeof(Configuration)).Serialize(writer, configuration);
            }
        }
    }
}