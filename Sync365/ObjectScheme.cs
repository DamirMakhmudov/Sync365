using System.Collections.Generic;

namespace Sync365
{
        /// <summary>
        /// Json универсальный класс
        /// </summary>
        public class ResponseJson
        {
            /// <summary>
            /// Имя системы, отправляющей запрос
            /// </summary>
            public string SystemName { get; set; }

            /// <summary>
            /// Пометка об успешном результате
            /// </summary>
            public bool Completed { get; set; }

            /// <summary>
            /// Текст ошибки, заполняется в случае неуспешного результата
            /// </summary>
            public string Result { get; set; }

            /// <summary>
            /// Дата получения результата
            /// </summary>
            public string Date { get; set; }
            /// <summary>
            /// GUID пакета загрузки в СЭТД, заполняется если найден
            /// </summary>
            public string O_Package_Unload { get; set; }

            /// <summary>
            /// Список объектов типа "jObject"
            /// </summary>
            public List<jObject> Objects { get; set; }
        }

        /// <summary>
        /// Json класс объекта
        /// </summary>
        public class jObject
        {
            /// <summary>
            /// GUID объекта в системе, отправляющей запрос
            /// </summary>
            public string ObjGuid { get; set; }

            /// <summary>
            /// GUID объекта в системе, принимающей запрос
            /// </summary>
            public string ObjGuidExternal { get; set; }

            /// <summary>
            /// Системное имя типа объекта
            /// </summary>
            public string ObjDefName { get; set; }

            /// <summary>
            /// Системное имя статуса, применение которого необходимо передать во внешнюю систему
            /// </summary>
            public string ObjStatus { get; set; }

            /// <summary>
            /// Дата изменения статуса
            /// </summary>
            public string StatusModifyTime { get; set; }

            /// <summary>
            /// Пользователь, изменивший статус
            /// </summary>
            public jUser StatusModifyUser { get; set; }

            /// <summary>
            /// Список атрибутов типа "jAttr"
            /// </summary>
            public List<jAttr> Attrs { get; set; }
        }

        /// <summary>
        /// Json класс атрибута
        /// </summary>
        public class jAttr
        {
            /// <summary>
            /// Описание атрибута
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Системное имя атрибута
            /// </summary>
            public string SysName { get; set; }

            /// <summary>
            /// Тип атрибута
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Значение атрибута
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Json Пакет передачи Реестра замечаний
        /// </summary>
        public class JsonPackageRZ
        {
            /// <summary>
            /// Имя системы (TDM365)
            /// </summary>
            public string SystemName { get; set; }

            /// <summary>
            /// GUID пакета выгрузки из внешней системы
            /// </summary>
            public string O_Package_Unload { get; set; }

            /// <summary>
            /// Реестр замечаний
            /// </summary>
            public jRZ RZ { get; set; }

            /// <summary>
            /// Список замечаний
            /// </summary>
            public List<jRemark> Remarks { get; set; }
        }

        /// <summary>
        /// Json Реестр замечаний
        /// </summary>
        public class jRZ
        {
            /// <summary>
            /// Описание объекта
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// GUID Реестра замечаний
            /// </summary>
            public string Guid { get; set; }

            /// <summary>
            /// Атрибут - GUID во внешней системе
            /// </summary>
            public string External_Guid { get; set; }

            /// <summary>
            /// Статус Реестра замечаний
            /// </summary>
            public jStatus Status { get; set; }

            /// <summary>
            /// Список файлов замечания
            /// </summary>
            public List<jFile> Files { get; set; }

            /// <summary>
            /// Экспертная группа Реестра замечаний
            /// </summary>
            public List<jExpert> Experts { get; set; }

            /// <summary>
            /// Атрибут - Создан
            /// </summary>
            public string ATTR_REGYSTRY_CREATION_DATE { get; set; }

            /// <summary>
            /// Атрибут - Документация (GUID в TDM365)
            /// </summary>
            public string ATTR_TechDoc_For_Observation { get; set; }

            /// <summary>
            /// Атрибут - Документация (GUID во внешней системе)
            /// </summary>
            public string TD_External_Guid { get; set; }

            /// <summary>
            /// Атрибут - Наименование
            /// </summary>
            public string ATTR_NAME_REGISTRY { get; set; }

            /// <summary>
            /// Атрибут - Номер
            /// </summary>
            public int ATTR_Registry_Num { get; set; }

            /// <summary>
            /// Атрибут - Завершить этап до
            /// </summary>
            public string ATTR_REGYSTRY_COMPLETE_THE_STAGE_BEFORE { get; set; }

            /// <summary>
            /// Атрибут - Инициировал
            /// </summary>
            public jUser ATTR_Registry_UserInitiated { get; set; }

            /// <summary>
            /// Атрибут - Запустить (план)
            /// </summary>
            public string ATTR_REGISTRY_LAUNCH_A_PLAN { get; set; }

            /// <summary>
            /// Атрибут - Запущен (факт)
            /// </summary>
            public string ATTR_REGISTRY_LAUNCHED_BY_THE_FACT { get; set; }

            /// <summary>
            /// Атрибут - Завершить (план)
            /// </summary>
            public string ATTR_REGISTRY_PLAN_DATE_OF_FINISH { get; set; }

            /// <summary>
            /// Атрибут - Завершил
            /// </summary>
            public jUser ATTR_Registry_UserFinished { get; set; }

            /// <summary>
            /// Атрибут - Завершен
            /// </summary>
            public string ATTR_REGISTRY_FACT_DATE_OF_FINISH { get; set; }

            /// <summary>
            /// Атрибут - Цикл
            /// </summary>
            public int ATTR_Registry_CycleNum { get; set; }
        }

        /// <summary>
        /// Json замечание
        /// </summary>
        public class jRemark
        {
            /// <summary>
            /// Описание объекта
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// GUID замечания
            /// </summary>
            public string Guid { get; set; }

            /// <summary>
            /// Атрибут - GUID во внешней системе
            /// </summary>
            public string External_Guid { get; set; }

            /// <summary>
            /// Статус Реестра замечаний
            /// </summary>
            public jStatus Status { get; set; }

            /// <summary>
            /// Список файлов замечания
            /// </summary>
            public List<jFile> Files { get; set; }

            /// <summary>
            /// Атрибут - Версия
            /// </summary>
            public int ATTR_TechDoc_Version { get; set; }

            /// <summary>
            /// Атрибут - Цикл
            /// </summary>
            public int ATTR_Registry_CycleNum { get; set; }

            /// <summary>
            /// Атрибут - Номер замечания
            /// </summary>
            public int ATTR_Remark_Num { get; set; }

            /// <summary>
            /// Атрибут - Замечание
            /// </summary>
            public string ATTR_REMARK_TYPE { get; set; }

            /// <summary>
            /// Атрибут - Описание
            /// </summary>
            public string ATTR_Remark { get; set; }

            /// <summary>
            /// Атрибут - Автор замечания
            /// </summary>
            public jUser ATTR_AUTHOR_ZM { get; set; }

            /// <summary>
            /// Атрибут - Дата замечания
            /// </summary>
            public string ATTR_Remark_Date { get; set; }

            /// <summary>
            /// Атрибут - Ответ
            /// </summary>
            public string ATTR_Answer_Type { get; set; }

            /// <summary>
            /// Атрибут - Обоснование
            /// </summary>
            public string ATTR_Answer { get; set; }

            /// <summary>
            /// Атрибут - Автор ответа
            /// </summary>
            public jUser ATTR_AUTHOR_ANSWER { get; set; }

            /// <summary>
            /// Атрибут - Дата ответа
            /// </summary>
            public string ATTR_Answer_Date { get; set; }
        }

        /// <summary>
        /// Json файл
        /// </summary>
        public class jFile
        {
            /// <summary>
            /// Имя файла
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Путь к файлу
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Md5 хэш файла
            /// </summary>
            public string Hash { get; set; }
        }

        /// <summary>
        /// Json Статус
        /// </summary>
        public class jStatus
        {
            /// <summary>
            /// Описание статуса
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Системное имя статуса
            /// </summary>
            public string Sysname { get; set; }

            /// <summary>
            /// Время изменения статуса
            /// </summary>
            public string StatusModifyTime { get; set; }

            /// <summary>
            /// Пользователь, изменивший статус
            /// </summary>
            public string StatusModifyUser { get; set; }
        }

        /// <summary>
        /// Json Эксперт
        /// </summary>
        public class jExpert
        {
            /// <summary>
            /// Описание эксперта
            /// </summary>
            public jUser User { get; set; }


            /// <summary>
            /// Закончил работу
            /// </summary>
            public bool Finished { get; set; }
        }

        /// <summary>
        /// Json Пользователь
        /// </summary>
        public class jUser
        {
            /// <summary>
            /// Описание пользователя
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Системное имя пользователя
            /// </summary>
            public string Sysname { get; set; }

            /// <summary>
            /// Системное имя пользователя во внешней системе
            /// </summary>
            public string SysnameExternal { get; set; }

            /// <summary>
            /// Фамилия
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Имя
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// Отчество
            /// </summary>
            public string MiddleName { get; set; }

            /// <summary>
            /// Телефон
            /// </summary>
            public string Tel { get; set; }

            /// <summary>
            /// Электронная почта
            /// </summary>
            public string Mail { get; set; }
        }
}
