-- Campagne de mesure corpus 1 — requêtes R1–R12 (méthodologie validée, doc/méthodilogie_pipeline.txt)
-- + C5 (validité des hash). Exécutées telles quelles contre la base produite par le scan.

-- R1 — Totaux et durée interne
SELECT COUNT(*)                                   AS observations,
       SUM(size)                                  AS octets_corpus,
       MIN(scanned_at)                            AS debut,
       MAX(scanned_at)                            AS fin,
       ROUND((julianday(MAX(scanned_at)) - julianday(MIN(scanned_at))) * 86400, 1) AS duree_s
FROM scan_observations;

-- R2 — Invariant 1:1 (les 6 tables de capacités)
SELECT (SELECT COUNT(*) FROM scan_observations) AS obs,
       (SELECT COUNT(*) FROM file_headers)      AS headers,
       (SELECT COUNT(*) FROM pe_info)           AS pe,
       (SELECT COUNT(*) FROM version_info)      AS version,
       (SELECT COUNT(*) FROM authenticode)      AS auth,
       (SELECT COUNT(*) FROM msi_properties)    AS msi,
       (SELECT COUNT(*) FROM appx_manifest)     AS appx;

-- R3 — Orphelins (répété pour chacune des 6 tables ; exemple file_headers)
SELECT COUNT(*) FROM file_headers h
LEFT JOIN scan_observations o ON o.id = h.observation_id
WHERE o.id IS NULL;

-- R4 — Répartition des conteneurs
SELECT COALESCE(container, '(null)') AS conteneur, COUNT(*) AS n,
       ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM file_headers), 2) AS pct
FROM file_headers GROUP BY container ORDER BY n DESC;

-- R5 — Présence des signaux par capacité
SELECT
  (SELECT COUNT(*) FROM version_info  WHERE product_name IS NOT NULL OR company_name IS NOT NULL
                                         OR product_version IS NOT NULL OR file_version IS NOT NULL) AS version_info_present,
  (SELECT COUNT(*) FROM authenticode  WHERE thumbprint IS NOT NULL)   AS certificats_observes,
  (SELECT COUNT(*) FROM msi_properties WHERE product_code IS NOT NULL) AS msi_renseignes,
  (SELECT COUNT(*) FROM appx_manifest WHERE name IS NOT NULL)          AS manifestes_appx;

-- R6 — Croisement conteneur × signaux
SELECT COALESCE(h.container, '(null)') AS conteneur,
       COUNT(*)                                        AS n,
       SUM(v.product_name IS NOT NULL)                 AS avec_product_name,
       SUM(a.thumbprint IS NOT NULL)                   AS avec_certificat,
       SUM(m.product_code IS NOT NULL)                 AS avec_msi_props,
       SUM(x.name IS NOT NULL)                         AS avec_appx_manifest
FROM file_headers h
JOIN version_info   v ON v.observation_id = h.observation_id
JOIN authenticode   a ON a.observation_id = h.observation_id
JOIN msi_properties m ON m.observation_id = h.observation_id
JOIN appx_manifest  x ON x.observation_id = h.observation_id
GROUP BY h.container ORDER BY n DESC;

-- R7 — Structure PE observée
SELECT machine, optional_header_magic, COUNT(*) AS n
FROM pe_info WHERE machine IS NOT NULL
GROUP BY machine, optional_header_magic ORDER BY n DESC;

-- R8 — Éditeurs signataires observés (fréquences brutes, aucune identification)
SELECT subject, COUNT(*) AS n FROM authenticode
WHERE subject IS NOT NULL GROUP BY subject ORDER BY n DESC LIMIT 25;

-- R9 — Taux de NULL par colonne (version_info)
SELECT COUNT(*) AS n,
       ROUND(100.0 * SUM(product_name   IS NULL) / COUNT(*), 2) AS pct_null_product_name,
       ROUND(100.0 * SUM(company_name   IS NULL) / COUNT(*), 2) AS pct_null_company_name,
       ROUND(100.0 * SUM(product_version IS NULL) / COUNT(*), 2) AS pct_null_product_version,
       ROUND(100.0 * SUM(file_version   IS NULL) / COUNT(*), 2) AS pct_null_file_version
FROM version_info;

-- R9bis — Taux de NULL par colonne (msi_properties)
SELECT COUNT(*) AS n,
       ROUND(100.0 * SUM(product_name     IS NULL) / COUNT(*), 2) AS pct_null_product_name,
       ROUND(100.0 * SUM(product_version  IS NULL) / COUNT(*), 2) AS pct_null_product_version,
       ROUND(100.0 * SUM(manufacturer     IS NULL) / COUNT(*), 2) AS pct_null_manufacturer,
       ROUND(100.0 * SUM(product_code     IS NULL) / COUNT(*), 2) AS pct_null_product_code,
       ROUND(100.0 * SUM(upgrade_code     IS NULL) / COUNT(*), 2) AS pct_null_upgrade_code,
       ROUND(100.0 * SUM(product_language IS NULL) / COUNT(*), 2) AS pct_null_product_language
FROM msi_properties;

-- R9ter — Taux de NULL par colonne (pe_info)
SELECT COUNT(*) AS n,
       ROUND(100.0 * SUM(machine               IS NULL) / COUNT(*), 2) AS pct_null_machine,
       ROUND(100.0 * SUM(subsystem             IS NULL) / COUNT(*), 2) AS pct_null_subsystem,
       ROUND(100.0 * SUM(optional_header_magic IS NULL) / COUNT(*), 2) AS pct_null_optional_header_magic
FROM pe_info;

-- R10 — Multiplicité des sha256
SELECT COUNT(*) AS observations, COUNT(DISTINCT sha256) AS contenus_distincts FROM scan_observations;
SELECT sha256, COUNT(*) AS n FROM scan_observations
GROUP BY sha256 HAVING n > 1 ORDER BY n DESC LIMIT 10;

-- R11 — Distribution des tailles
SELECT CASE WHEN size < 1048576 THEN 'a. < 1 Mo'
            WHEN size < 10485760 THEN 'b. 1-10 Mo'
            WHEN size < 104857600 THEN 'c. 10-100 Mo'
            WHEN size < 1073741824 THEN 'd. 100 Mo - 1 Go'
            ELSE 'e. > 1 Go' END AS classe,
       COUNT(*) AS n, ROUND(SUM(size) / 1048576.0) AS mo_total
FROM scan_observations GROUP BY classe ORDER BY classe;

-- R12 — Taille de la base
SELECT page_count * page_size / 1048576.0 AS taille_base_mo
FROM pragma_page_count(), pragma_page_size();

-- C5 — Validité des hash
SELECT COUNT(*) FROM scan_observations WHERE length(sha256) <> 64;
