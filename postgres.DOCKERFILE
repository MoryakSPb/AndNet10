FROM postgres:15.3

RUN localedef -i ru_RU -c -f UTF-8 -A /usr/share/locale/locale.alias ru_RU.UTF-8
ENV LANG ru_RU.utf8

RUN apt-get update && \
    apt-get install -y --no-install-recommends postgresql-15-rum && \
    apt-get purge -y --auto-remove && \
	rm -rf /var/lib/apt/lists/*