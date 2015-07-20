package com.game.bizModule.human.entity;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.Table;

/**
 * 玩家角色入口实体
 *
 * @author hjj2019
 * @since 2015/7/11
 *
 */
@Entity
@Table(name = "t_human")
public class HumanEntryEntity {
    /** 玩家角色 UId */
    @Id @Column(name = "human_uid")
    public Long _humanUId = 0L;
    /** 玩家角色名称 */
    @Column(name = "human_name", length = 32, updatable = false)
    public String _humanName = null;
    /** 平台 UId 字符串 */
    @Column(name = "platform_uid_str", length = 64, updatable = false)
    public String _platformUIdStr = null;
    /** 服务器名称 */
    @Column(name = "server_name", length = 16, updatable = false)
    public String _serverName = null;
    /** 角色等级 */
    @Column(name = "human_level", updatable = false)
    public Integer _humanLevel = 0;
}
