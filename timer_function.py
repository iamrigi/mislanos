import logging
from archival_logic import archive_old_records

def main(mytimer: func.TimerRequest) -> None:
    logging.info("Archival function triggered.")
    archive_old_records()
    logging.info("Archival process completed.")