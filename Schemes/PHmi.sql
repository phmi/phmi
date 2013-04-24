/*==============================================================*/
/* Database name:  PHmi                                         */
/*==============================================================*/


/*==============================================================*/
/* Table: alarm_categories                                      */
/*==============================================================*/
create table alarm_categories (
id                   serial not null,
name                 text                 not null,
description          text                 null,
time_to_store        int8                 null,
constraint pk_alarm_categories primary key (id)
);

/*==============================================================*/
/* Table: alarm_tags                                            */
/*==============================================================*/
create table alarm_tags (
id                   serial not null,
name                 text                 not null,
ref_dig_tags         int4                 not null,
location             text                 null,
description          text                 null,
privilege            int4                 null,
ref_categories       int4                 not null,
acknowledgeable      bool                 not null,
constraint pk_alarm_tags primary key (id)
);

/*==============================================================*/
/* Table: dig_tags                                              */
/*==============================================================*/
create table dig_tags (
id                   serial not null,
name                 text                 not null,
ref_io_devices       int4                 not null,
device               text                 not null,
description          text                 null,
can_read             bool                 not null,
constraint pk_dig_tags primary key (id)
);

/*==============================================================*/
/* Table: io_devices                                            */
/*==============================================================*/
create table io_devices (
id                   serial not null,
name                 text                 not null,
type                 text                 not null,
options              text                 null,
constraint pk_io_devices primary key (id)
);

/*==============================================================*/
/* Table: logs                                                  */
/*==============================================================*/
create table logs (
id                   serial not null,
name                 text                 not null,
time_to_store        int8                 null,
constraint pk_logs primary key (id)
);

/*==============================================================*/
/* Table: num_tag_types                                         */
/*==============================================================*/
create table num_tag_types (
id                   int4                 not null,
name                 text                 not null,
constraint pk_num_tag_types primary key (id)
);

/*==============================================================*/
/* Table: num_tags                                              */
/*==============================================================*/
create table num_tags (
id                   serial not null,
name                 text                 not null,
ref_io_devices       int4                 not null,
ref_tag_types        int4                 not null,
device               text                 not null,
description          text                 null,
can_read             bool                 not null,
eng_unit             text                 null,
raw_min              float8               null,
raw_max              float8               null,
eng_min              float8               null,
eng_max              float8               null,
format               text                 null,
constraint pk_num_tags primary key (id)
);

/*==============================================================*/
/* Table: settings                                              */
/*==============================================================*/
create table settings (
id                   int4                 not null,
server               text                 null,
stand_by_server      text                 null,
guid                 text                 not null,
guid2                text                 not null,
phmi_guid            text                 not null,
constraint pk_settings primary key (id)
);

/*==============================================================*/
/* Table: trend_categories                                      */
/*==============================================================*/
create table trend_categories (
id                   serial not null,
name                 text                 not null,
time_to_store        int8                 null,
period               int8                 not null,
constraint pk_trend_categories primary key (id)
);

/*==============================================================*/
/* Table: trend_tags                                            */
/*==============================================================*/
create table trend_tags (
id                   serial not null,
name                 text                 not null,
ref_num_tags         int4                 not null,
description          text                 null,
ref_categories       int4                 not null,
ref_triggers         int4                 null,
constraint pk_trend_tags primary key (id)
);

/*==============================================================*/
/* Table: users                                                 */
/*==============================================================*/
create table users (
id                   serial not null,
name                 text                 not null,
description          text                 not null,
photo                bytea                null,
password             text                 null,
enabled              bool                 not null,
can_change           bool                 not null,
privilege            int4                 null,
constraint pk_users primary key (id)
);

/*==============================================================*/
/* Index: i_users_name                                          */
/*==============================================================*/
create unique index i_users_name on users using btree (
name
);

alter table alarm_tags
   add constraint fk_alarm_ta_reference_dig_tags foreign key (ref_dig_tags)
      references dig_tags (id)
      on delete cascade on update cascade;

alter table alarm_tags
   add constraint fk_alarm_ta_reference_alarm_ca foreign key (ref_categories)
      references alarm_categories (id)
      on delete cascade on update cascade;

alter table dig_tags
   add constraint fk_dig_tags_reference_io_devic foreign key (ref_io_devices)
      references io_devices (id)
      on delete cascade on update cascade;

alter table num_tags
   add constraint fk_num_tags_reference_io_devic foreign key (ref_io_devices)
      references io_devices (id)
      on delete cascade on update cascade;

alter table num_tags
   add constraint fk_num_tags_reference_num_tag_ foreign key (ref_tag_types)
      references num_tag_types (id)
      on delete cascade on update cascade;

alter table trend_tags
   add constraint fk_trend_ta_reference_trend_ca foreign key (ref_categories)
      references trend_categories (id)
      on delete cascade on update cascade;

alter table trend_tags
   add constraint fk_trend_ta_reference_dig_tags foreign key (ref_triggers)
      references dig_tags (id)
      on delete cascade on update cascade;

alter table trend_tags
   add constraint fk_trend_ta_reference_num_tags foreign key (ref_num_tags)
      references num_tags (id)
      on delete cascade on update cascade;

