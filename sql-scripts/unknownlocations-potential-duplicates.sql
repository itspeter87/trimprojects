SELECT 

    lcname,

    COUNT(*) AS “Potential Duplicates”

FROM tslocation

WHERE lctype = 0

GROUP BY lcname

HAVING COUNT(*) > 1

ORDER BY “Potential Duplicates” DESC;