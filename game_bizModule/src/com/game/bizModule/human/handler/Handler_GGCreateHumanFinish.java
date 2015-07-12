package com.game.bizModule.human.handler;

import com.game.bizModule.human.HumanStateTable;
import com.game.bizModule.human.msg.*;
import com.game.gameServer.msg.AbstractGGMsgHandler;

/**
 * 查询玩家角色入口列表完成
 *
 * @author hjj2019
 * @since 2015/7/11
 *
 */
public class Handler_GGCreateHumanFinish extends AbstractGGMsgHandler<GGCreateHumanFinish> {
    @Override
    public void handle(GGCreateHumanFinish ggMSG) {
        if (ggMSG == null ||
            ggMSG._p == null) {
            // 如果参数对象为空,
            // 则直接退出!
            return;
        }

        // 获取角色状态表
        HumanStateTable hStateTbl = ggMSG._p.getPropValOrCreate(HumanStateTable.class);
        // 建角过程执行完毕
        hStateTbl._execCreateHuman = false;

        // 发送 GC 消息
        this.sendMsgToClient(
            new GCCreateHuman(ggMSG._success),
            ggMSG._p
        );
    }
}