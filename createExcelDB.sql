CREATE DATABASE IF NOT EXISTS DB_Exel;
USE DB_Exel;

-- Таблица банков
CREATE TABLE banks (
    bank_id INT AUTO_INCREMENT PRIMARY KEY,
    bank_name VARCHAR(255)
);

-- Таблица классов
CREATE TABLE classes (
    class_id INT AUTO_INCREMENT PRIMARY KEY,
    class_number INT,
    class_name VARCHAR(255)
);

-- Таблица информации о ведомости
CREATE TABLE reports_info (
    info_id INT AUTO_INCREMENT PRIMARY KEY,
    creation_date TIMESTAMP,
    currency VARCHAR(50),
    report_name VARCHAR(255),  -- Название ведомости
    start_date VARCHAR(255),           -- Дата начала периода
    end_date VARCHAR(255)               -- Дата окончания периода
);

-- Таблица отчетов
CREATE TABLE reports (
    report_id INT AUTO_INCREMENT PRIMARY KEY,
    bank_id INT,
    info_id INT,
    FOREIGN KEY (bank_id) REFERENCES banks(bank_id),
    FOREIGN KEY (info_id) REFERENCES reports_info(info_id)
);

-- Таблица, связывающая отчеты и классы
CREATE TABLE report_class (
    report_id INT,
    class_id INT,
    PRIMARY KEY (report_id, class_id),
    FOREIGN KEY (report_id) REFERENCES reports(report_id),
    FOREIGN KEY (class_id) REFERENCES classes(class_id)
);

-- Таблица записей в счете
CREATE TABLE account_entries (
    entry_id INT AUTO_INCREMENT PRIMARY KEY,
    report_id INT,
    class_id INT, -- Добавлен внешний ключ для связи с классом
    b_sch VARCHAR(255),
    vhodyaschee_saldo_aktiv VARCHAR(255),
    vhodyaschee_saldo_passiv VARCHAR(255),
    oborot_debet VARCHAR(255),
    oborot_kredit VARCHAR(255),
    ishodyaschee_saldo_aktiv VARCHAR(255),
    ishodyaschee_saldo_passiv VARCHAR(255),
    FOREIGN KEY (report_id) REFERENCES reports(report_id),
    FOREIGN KEY (class_id) REFERENCES classes(class_id) -- Добавлен внешний ключ для связи с классом
);

USE DB_Exel;



CREATE OR REPLACE VIEW AllDataView AS
SELECT DISTINCT
    B.bank_id,
    B.bank_name,
    C.class_id,
    C.class_number,
    C.class_name,
    RI.info_id,
    RI.creation_date,
    RI.currency,
    RI.report_name,
    RI.start_date,
    RI.end_date,
    R.report_id,
    R.bank_id AS report_bank_id,
    R.info_id AS report_info_id,
    RC.report_id AS rc_report_id,
    RC.class_id AS rc_class_id,
    AE.entry_id,
    AE.report_id AS ae_report_id,
    AE.class_id AS ae_class_id,
    AE.b_sch,
    AE.vhodyaschee_saldo_aktiv,
    AE.vhodyaschee_saldo_passiv,
    AE.oborot_debet,
    AE.oborot_kredit,
    AE.ishodyaschee_saldo_aktiv,
    AE.ishodyaschee_saldo_passiv
FROM
    banks B
LEFT JOIN
    reports R ON B.bank_id = R.bank_id
LEFT JOIN
    reports_info RI ON R.info_id = RI.info_id
LEFT JOIN
    report_class RC ON R.report_id = RC.report_id
LEFT JOIN
    classes C ON RC.class_id = C.class_id
LEFT JOIN
    account_entries AE ON R.report_id = AE.report_id AND C.class_id = AE.class_id;




-- Выбор всех полей из VIEW
SELECT * FROM AllDataView;
SELECT * FROM AllDataView where bank_name=6;
select * from AllDataView where report_name="Оборотная ведомость по балансовым счетам";
select * from AllDataView where class_name="КЛАСС  1  Денежные средства, драгоценные металлы и межбанковские операции";


