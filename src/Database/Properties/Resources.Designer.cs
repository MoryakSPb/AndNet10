﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AndNet.Manager.Database.Properties {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AndNet.Manager.Database.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на set client_min_messages = WARNING;
        ///DROP TABLE IF EXISTS &quot;RegistryWorker&quot;.qrtz_fired_triggers;
        ///DROP TABLE IF EXISTS &quot;RegistryWorker&quot;.qrtz_paused_trigger_grps;
        ///DROP TABLE IF EXISTS &quot;RegistryWorker&quot;.qrtz_scheduler_state;
        ///DROP TABLE IF EXISTS &quot;RegistryWorker&quot;.qrtz_locks;
        ///DROP TABLE IF EXISTS &quot;RegistryWorker&quot;.qrtz_simprop_triggers;
        ///DROP TABLE IF EXISTS &quot;RegistryWorker&quot;.qrtz_simple_triggers;
        ///DROP TABLE IF EXISTS &quot;RegistryWorker&quot;.qrtz_cron_triggers;
        ///DROP TABLE IF EXISTS &quot;RegistryWorker&quot;.qrtz_blob_triggers; [остаток строки не уместился]&quot;;.
        /// </summary>
        internal static string tables_postgres_down {
            get {
                return ResourceManager.GetString("tables_postgres_down", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на set client_min_messages = NOTICE;
        ///
        ///CREATE TABLE &quot;RegistryWorker&quot;.qrtz_job_details
        ///  (
        ///    sched_name TEXT NOT NULL,
        ///	job_name  TEXT NOT NULL,
        ///    job_group TEXT NOT NULL,
        ///    description TEXT NULL,
        ///    job_class_name   TEXT NOT NULL, 
        ///    is_durable BOOL NOT NULL,
        ///    is_nonconcurrent BOOL NOT NULL,
        ///    is_update_data BOOL NOT NULL,
        ///	requests_recovery BOOL NOT NULL,
        ///    job_data BYTEA NULL,
        ///    PRIMARY KEY (sched_name,job_name,job_group)
        ///);
        ///
        ///CREATE TABLE &quot;RegistryWorker&quot;.qrtz_triggers
        ///  (        /// [остаток строки не уместился]&quot;;.
        /// </summary>
        internal static string tables_postgres_up {
            get {
                return ResourceManager.GetString("tables_postgres_up", resourceCulture);
            }
        }
    }
}
